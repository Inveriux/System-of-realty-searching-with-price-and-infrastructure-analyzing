using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Places.Overpass.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Places.Overpass;

public sealed class OverpassPlacesProvider(
    IOverpassApiClient overpassApiClient,
    IOverpassPoiMapper overpassPoiMapper,
    IMemoryCache memoryCache,
    IOptions<OverpassOptions> overpassOptions,
    IOptions<InfrastructureScoringOptions> scoringOptions,
    ILogger<OverpassPlacesProvider> logger) : IPlacesProvider
{
    private const string ProviderName = "osm-overpass";
    private const string CacheKeyVersion = "v2";
    private const decimal MetroPrimaryRadiusMeters = 2000m;
    private const decimal MetroNearestBandMeters = 100m;
    private const int AdaptiveRadiusStepMeters = 500;
    private const int AdaptiveKilometerStepMeters = 1000;
    private const int TransportAdaptiveEnoughCount = 18;
    private const int HealthcareAdaptiveEnoughCount = 10;
    private const int CommerceAdaptiveEnoughCount = 18;
    private const int LeisureAdaptiveEnoughCount = 14;
    private static readonly QueryGroup[] QueryGroups =
    [
        new(["transport"], 2000,
        [
            new() { Key = "highway", Values = ["bus_stop"] },
            new() { Key = "railway", Values = ["tram_stop", "station", "halt"] },
            new() { Key = "amenity", Values = ["bus_station", "fuel", "car_wash"] },
            new() { Key = "shop", Values = ["car_repair"] },
            new() { Key = "craft", Values = ["car_repair"] }
        ], true, 500, AdaptiveRadiusStepMeters, TransportAdaptiveEnoughCount),
        new(["transport"], 500,
        [
            new() { Key = "amenity", Values = ["parking"] }
        ]),
        new(["transport"], 12000,
        [
            new() { Key = "railway", Values = ["subway_entrance", "subway", "station"] },
            new() { Key = "station", Values = ["subway"] }
        ]),
        new(["education"], 2000),
        new(["healthcare"], 1500,
        [
            new() { Key = "amenity", Values = ["clinic", "doctors", "pharmacy"] },
            new() { Key = "healthcare", Values = ["clinic"] }
        ], true, 500, AdaptiveRadiusStepMeters, HealthcareAdaptiveEnoughCount),
        new(["healthcare"], 5000,
        [
            new() { Key = "amenity", Values = ["hospital"] },
            new() { Key = "healthcare", Values = ["hospital"] }
        ]),
        new(["commerce"], 1800,
        null, true, 1000, AdaptiveKilometerStepMeters, CommerceAdaptiveEnoughCount),
        new(["leisure"], 3000,
        null, true, 1000, AdaptiveKilometerStepMeters, LeisureAdaptiveEnoughCount)
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<OverpassTagFilter>> CategoryFilters =
        new Dictionary<string, IReadOnlyCollection<OverpassTagFilter>>(StringComparer.OrdinalIgnoreCase)
        {
            ["education"] =
            [
                new() { Key = "amenity", Values = ["school", "kindergarten", "college", "university"] }
            ],
            ["healthcare"] =
            [
                new() { Key = "amenity", Values = ["hospital", "clinic", "doctors", "pharmacy"] },
                new() { Key = "healthcare", Values = ["hospital", "clinic"] }
            ],
            ["transport"] =
            [
                new() { Key = "highway", Values = ["bus_stop"] },
                new() { Key = "railway", Values = ["tram_stop", "station", "halt", "subway_entrance", "subway"] },
                new() { Key = "amenity", Values = ["bus_station", "parking", "fuel", "car_wash"] },
                new() { Key = "station", Values = ["subway"] },
                new() { Key = "shop", Values = ["car_repair"] },
                new() { Key = "craft", Values = ["car_repair"] }
            ],
            ["commerce"] =
            [
                new() { Key = "shop", Values = ["supermarket", "convenience", "mall", "chemist", "department_store", "greengrocer", "bakery", "clothes", "electronics", "hardware", "doityourself", "hairdresser", "beauty", "cosmetics"] },
                new() { Key = "amenity", Values = ["beauty_salon", "hairdresser", "barber"] },
                new() { Key = "beauty", Values = ["hairdresser", "beautician", "nails", "spa", "massage"] }
            ],
            ["leisure"] =
            [
                new() { Key = "leisure", Values = ["park", "garden", "nature_reserve", "common", "recreation_ground", "dog_park", "fitness_centre"] },
                new() { Key = "sport", Values = ["fitness"] },
                new() { Key = "amenity", Values = ["cinema", "cafe", "restaurant", "fast_food", "bar", "theatre", "library", "arts_centre", "place_of_worship"] },
                new() { Key = "tourism", Values = ["museum", "gallery", "attraction", "artwork", "viewpoint"] },
                new() { Key = "historic", Values = ["memorial", "monument", "castle", "ruins"] },
                new() { Key = "building", Values = ["church", "cathedral", "chapel", "mosque", "synagogue", "temple"] },
                new() { Key = "natural", Values = ["wood", "beach", "grassland", "scrub", "heath", "wetland"] },
                new() { Key = "landuse", Values = ["forest", "village_green", "recreation_ground", "meadow"] },
                new() { Key = "boundary", Values = ["national_park", "protected_area"] }
            ]
        };

    public async Task<IReadOnlyCollection<ExternalPoiResult>> GetNearbyPoisAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<InfrastructureCategory> categories,
        int limit,
        CancellationToken cancellationToken)
    {
        var requestedRadius = radiusMeters > 0 ? radiusMeters : overpassOptions.Value.DefaultRadiusMeters;
        var boundedRadius = Math.Clamp(requestedRadius, 100, Math.Min(overpassOptions.Value.MaxRadiusMeters, scoringOptions.Value.MaxRadiusMeters));
        var categoryNames = categories.Count == 0
            ? CategoryFilters.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray()
            : categories
                .Select(static category => category.ToString().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        var cacheKey = BuildCacheKey((decimal)latitude, (decimal)longitude, boundedRadius, categoryNames);

        if (memoryCache.TryGetValue<IReadOnlyCollection<ExternalPoiResult>>(cacheKey, out var cached) && cached is not null)
            return cached;

        var rawElements = new List<OverpassElement>();
        var groupResultCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var queryGroup in QueryGroups)
        {
            var groupCategories = categoryNames
                .Where(category => queryGroup.Categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (groupCategories.Length == 0)
                continue;

            var filters = queryGroup.Filters?.Count > 0
                ? queryGroup.Filters.ToArray()
                : groupCategories
                    .SelectMany(static category => CategoryFilters.TryGetValue(category, out var value) ? value : [])
                    .GroupBy(static x => x.Key)
                    .Select(group => new OverpassTagFilter
                    {
                        Key = group.Key,
                        Values = group.SelectMany(static x => x.Values).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                    })
                    .ToArray();

            if (filters.Length == 0)
                continue;

            var groupRadius = Math.Min(boundedRadius, queryGroup.MaxRadiusMeters);
            var groupRaw = queryGroup.UseAdaptiveRadius
                ? await QueryAdaptiveNearbyAsync(
                    (decimal)latitude,
                    (decimal)longitude,
                    filters,
                    groupRadius,
                    queryGroup.StartRadiusMeters ?? Math.Min(AdaptiveRadiusStepMeters, groupRadius),
                    queryGroup.RadiusStepMeters ?? AdaptiveRadiusStepMeters,
                    queryGroup.EnoughMappedCount ?? 1,
                    cancellationToken)
                : await overpassApiClient.QueryNearbyAsync(
                    (decimal)latitude,
                    (decimal)longitude,
                    groupRadius,
                    filters,
                    cancellationToken);

            rawElements.AddRange(groupRaw);

            foreach (var category in groupCategories)
            {
                groupResultCounts[category] = groupResultCounts.TryGetValue(category, out var existing)
                    ? existing + groupRaw.Count
                    : groupRaw.Count;
            }
        }

        IReadOnlyCollection<ExternalPoiResult> deduplicated;

        try
        {
            var mappedPois = rawElements
                .Select(element => overpassPoiMapper.Map(element, (decimal)latitude, (decimal)longitude))
                .Where(static x => x is not null)
                .Select(static x => x!)
                .GroupBy(static x => BuildDeduplicationKey(x))
                .Select(static group => group.OrderBy(x => x.Poi.DistanceMeters).First())
                .OrderBy(static x => x.Poi.DistanceMeters)
                .ToArray();

            mappedPois = NormalizeMetroPois(mappedPois);

            deduplicated = mappedPois
                .Select(static x => x.Poi)
                .ToArray();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Overpass nearby search mapping failed. Returning empty result set.");
            deduplicated = [];
        }

        var shouldCache = ShouldCacheResult(categoryNames, groupResultCounts, deduplicated.Count);
        if (shouldCache)
        {
            memoryCache.Set(cacheKey, deduplicated, TimeSpan.FromMinutes(scoringOptions.Value.CacheMinutes));
        }

        logger.LogInformation(
            "Overpass nearby search completed. Latitude: {Latitude}, Longitude: {Longitude}, Radius: {Radius}, RawCount: {RawCount}, Count: {Count}, GroupCounts: {@GroupCounts}, Cached: {Cached}",
            latitude,
            longitude,
            boundedRadius,
            rawElements.Count,
            deduplicated.Count,
            groupResultCounts,
            shouldCache);

        return deduplicated;
    }

    private static string BuildCacheKey(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        IReadOnlyCollection<string> categories)
        => $"{ProviderName}:{CacheKeyVersion}:{decimal.Round(latitude, 5)}:{decimal.Round(longitude, 5)}:{radiusMeters}:{string.Join('|', categories.Order(StringComparer.OrdinalIgnoreCase))}";

    private static string BuildDeduplicationKey(OverpassMappedPoi poi)
    {
        var normalizedName = string.IsNullOrWhiteSpace(poi.Poi.Name)
            ? poi.SubcategoryCode
            : poi.Poi.Name.Trim().ToLowerInvariant();
        return $"{poi.CategoryCode}:{poi.SubcategoryCode}:{normalizedName}:{decimal.Round(poi.Poi.Latitude, 5)}:{decimal.Round(poi.Poi.Longitude, 5)}";
    }

    private static OverpassMappedPoi[] NormalizeMetroPois(IReadOnlyCollection<OverpassMappedPoi> mappedPois)
    {
        var metroPois = mappedPois
            .Where(static poi => poi.CategoryCode.Equals("transport", StringComparison.OrdinalIgnoreCase)
                                 && poi.SubcategoryCode.Equals("metro_station", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static poi => poi.Poi.DistanceMeters)
            .ToArray();

        if (metroPois.Length == 0)
            return mappedPois.ToArray();

        var metroWithinPrimary = metroPois
            .Where(static poi => poi.Poi.DistanceMeters <= MetroPrimaryRadiusMeters)
            .ToArray();

        var allowedMetro = metroWithinPrimary.Length > 0
            ? metroWithinPrimary
            : metroPois
                .Where(poi => poi.Poi.DistanceMeters <= metroPois[0].Poi.DistanceMeters + MetroNearestBandMeters)
                .ToArray();

        var allowedMetroIds = allowedMetro
            .Select(static poi => poi.Poi.ExternalId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return mappedPois
            .Where(poi => !poi.CategoryCode.Equals("transport", StringComparison.OrdinalIgnoreCase)
                          || !poi.SubcategoryCode.Equals("metro_station", StringComparison.OrdinalIgnoreCase)
                          || allowedMetroIds.Contains(poi.Poi.ExternalId))
            .ToArray();
    }

    private static bool ShouldCacheResult(
        IReadOnlyCollection<string> requestedCategories,
        IReadOnlyDictionary<string, int> groupResultCounts,
        int finalCount)
    {
        if (finalCount == 0)
            return false;

        var requested = requestedCategories
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (requested.Length <= 1)
            return true;

        var zeroGroups = requested.Count(category => groupResultCounts.TryGetValue(category, out var count) && count == 0);
        var nonZeroGroups = requested.Count(category => groupResultCounts.TryGetValue(category, out var count) && count > 0);

        if (zeroGroups > 0 && nonZeroGroups > 0)
            return false;

        return true;
    }

    private async Task<IReadOnlyCollection<OverpassElement>> QueryAdaptiveNearbyAsync(
        decimal latitude,
        decimal longitude,
        IReadOnlyCollection<OverpassTagFilter> filters,
        int maxRadiusMeters,
        int startRadiusMeters,
        int stepMeters,
        int enoughMappedCount,
        CancellationToken cancellationToken)
    {
        var effectiveStart = Math.Clamp(startRadiusMeters, 100, maxRadiusMeters);
        var effectiveStep = Math.Max(stepMeters, 100);
        var targetCount = Math.Max(enoughMappedCount, 1);
        var aggregated = new Dictionary<string, OverpassElement>(StringComparer.OrdinalIgnoreCase);

        for (var radius = effectiveStart; radius <= maxRadiusMeters; radius += effectiveStep)
        {
            var stepResult = await overpassApiClient.QueryNearbyAsync(
                latitude,
                longitude,
                radius,
                filters,
                cancellationToken);

            foreach (var element in stepResult)
            {
                aggregated[$"{element.Type}/{element.Id}"] = element;
            }

            var mappedCount = aggregated.Values
                .Select(element => overpassPoiMapper.Map(element, latitude, longitude))
                .Where(static x => x is not null)
                .Select(static x => x!)
                .GroupBy(static x => BuildDeduplicationKey(x))
                .Count();

            if (mappedCount >= targetCount)
                break;
        }

        return aggregated.Values.ToArray();
    }

    private sealed record QueryGroup(
        IReadOnlyCollection<string> Categories,
        int MaxRadiusMeters,
        IReadOnlyCollection<OverpassTagFilter>? Filters = null,
        bool UseAdaptiveRadius = false,
        int? StartRadiusMeters = null,
        int? RadiusStepMeters = null,
        int? EnoughMappedCount = null);
}
