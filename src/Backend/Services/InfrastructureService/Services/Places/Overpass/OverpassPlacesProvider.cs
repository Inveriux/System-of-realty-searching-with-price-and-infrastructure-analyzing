using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Places.Overpass.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Places.Overpass;

public sealed class OverpassPlacesProvider(
    IOverpassApiClient overpassApiClient,
    IMemoryCache memoryCache,
    IOptions<InfrastructureScoringOptions> scoringOptions,
    ILogger<OverpassPlacesProvider> logger) : IPlacesProvider
{
    private const string ProviderName = "Overpass";

    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<OverpassTagFilter>> CategoryFilters =
        new Dictionary<string, IReadOnlyCollection<OverpassTagFilter>>(StringComparer.OrdinalIgnoreCase)
        {
            ["education"] =
            [
                new() { Key = "amenity", Values = ["school", "kindergarten", "college", "university"] },
                new() { Key = "building", Values = ["school", "kindergarten", "college", "university"] }
            ],
            ["healthcare"] =
            [
                new() { Key = "amenity", Values = ["hospital", "clinic", "doctors", "pharmacy", "dentist"] },
                new() { Key = "healthcare", Values = ["hospital", "clinic", "doctor", "pharmacy", "dentist"] }
            ],
            ["transport"] =
            [
                new() { Key = "public_transport", Values = ["platform", "station", "stop_position"] },
                new() { Key = "highway", Values = ["bus_stop"] },
                new() { Key = "railway", Values = ["station", "halt", "tram_stop", "subway_entrance"] },
                new() { Key = "amenity", Values = ["bus_station"] }
            ],
            ["commerce"] =
            [
                new() { Key = "shop", Values = ["supermarket", "convenience", "mall", "department_store", "grocery", "wholesale", "general"] },
                new() { Key = "amenity", Values = ["marketplace"] },
                new() { Key = "building", Values = ["retail", "supermarket", "commercial"] }
            ],
            ["leisure"] =
            [
                new() { Key = "leisure", Values = ["park", "fitness_centre", "sports_centre", "playground", "stadium", "garden", "swimming_pool", "pitch", "fitness_station"] },
                new() { Key = "amenity", Values = ["cinema", "cafe", "theatre", "restaurant", "fast_food", "bar", "pub", "library", "arts_centre"] },
                new() { Key = "tourism", Values = ["museum", "attraction", "gallery"] },
                new() { Key = "building", Values = ["sports_hall", "stadium", "museum", "theatre"] },
                new() { Key = "natural", Values = ["beach"] },
                new() { Key = "place", Values = ["square"] }
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
        var boundedRadius = Math.Clamp(radiusMeters, 100, scoringOptions.Value.MaxRadiusMeters);
        var boundedCount = Math.Clamp(limit, 1, scoringOptions.Value.MaxPoiCount);
        var categoryNames = categories
            .Select(static category => category.ToString().ToLowerInvariant())
            .ToArray();
        var cacheKey = BuildCacheKey((decimal)latitude, (decimal)longitude, boundedRadius, boundedCount, categoryNames);

        if (memoryCache.TryGetValue<IReadOnlyCollection<ExternalPoiResult>>(cacheKey, out var cached) && cached is not null)
            return cached;

        var filters = categoryNames
            .SelectMany(static category => CategoryFilters.TryGetValue(category, out var value) ? value : [])
            .GroupBy(static x => x.Key)
            .Select(group => new OverpassTagFilter
            {
                Key = group.Key,
                Values = group.SelectMany(static x => x.Values).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            })
            .ToArray();

        var raw = await overpassApiClient.QueryNearbyAsync(
            (decimal)latitude,
            (decimal)longitude,
            boundedRadius,
            filters,
            cancellationToken);

        var deduplicated = raw
            .Select(element => Map(element, (decimal)latitude, (decimal)longitude))
            .Where(static x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(static x => BuildDeduplicationKey(x))
            .Select(static group => group.OrderBy(x => x.DistanceMeters).First())
            .OrderBy(static x => x.DistanceMeters)
            .Take(boundedCount)
            .ToArray();

        memoryCache.Set(cacheKey, deduplicated, TimeSpan.FromMinutes(scoringOptions.Value.CacheMinutes));
        logger.LogInformation(
            "Overpass nearby search completed. Latitude: {Latitude}, Longitude: {Longitude}, Radius: {Radius}, Count: {Count}",
            latitude,
            longitude,
            boundedRadius,
            deduplicated.Length);

        return deduplicated;
    }

    private static ExternalPoiResult Map(OverpassElement element, decimal latitude, decimal longitude)
    {
        var tags = element.Tags;
        return new ExternalPoiResult
        {
            ExternalId = $"{element.Type}/{element.Id}",
            Name = tags.TryGetValue("name", out var name) ? name : "POI",
            AddressText = tags.TryGetValue("addr:full", out var fullAddress)
                ? fullAddress
                : tags.TryGetValue("addr:street", out var street)
                    ? street
                    : null,
            Latitude = element.Latitude!.Value,
            Longitude = element.Longitude!.Value,
            DistanceMeters = CalculateDistanceMeters(latitude, longitude, element.Latitude.Value, element.Longitude.Value),
            Source = ProviderName,
            RawCategory = null,
            RawCategories = [],
            Tags = tags
        };
    }

    private static string BuildCacheKey(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        int maxPoiCount,
        IReadOnlyCollection<string> categories)
        => $"{ProviderName}:{decimal.Round(latitude, 4)}:{decimal.Round(longitude, 4)}:{radiusMeters}:{maxPoiCount}:{string.Join('|', categories.Order(StringComparer.OrdinalIgnoreCase))}";

    private static string BuildDeduplicationKey(ExternalPoiResult poi)
        => $"{poi.ExternalId ?? string.Empty}:{poi.Name.Trim().ToLowerInvariant()}:{decimal.Round(poi.Latitude, 5)}:{decimal.Round(poi.Longitude, 5)}";

    private static decimal CalculateDistanceMeters(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double earthRadiusMeters = 6_371_000d;

        var lat1 = DegreesToRadians((double)latitude1);
        var lon1 = DegreesToRadians((double)longitude1);
        var lat2 = DegreesToRadians((double)latitude2);
        var lon2 = DegreesToRadians((double)longitude2);

        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;

        var a = Math.Pow(Math.Sin(dLat / 2), 2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return decimal.Round((decimal)(earthRadiusMeters * c), 2);
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;
}
