using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Places.Geoapify.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Places.Geoapify;

public sealed class GeoapifyPlacesProvider(
    IGeoapifyPlacesClient geoapifyPlacesClient,
    IMemoryCache memoryCache,
    IOptions<InfrastructureScoringOptions> scoringOptions,
    ILogger<GeoapifyPlacesProvider> logger) : IPlacesProvider
{
    private const string ProviderName = "Geoapify";

    private static readonly IReadOnlyDictionary<InfrastructureCategory, IReadOnlyCollection<string>> CategoryMap =
        new Dictionary<InfrastructureCategory, IReadOnlyCollection<string>>
        {
            [InfrastructureCategory.Education] = ["education.school", "building.kindergarten", "building.university"],
            [InfrastructureCategory.Healthcare] = ["healthcare.hospital", "healthcare.pharmacy", "building.healthcare"],
            [InfrastructureCategory.Transport] = ["public_transport", "railway"],
            [InfrastructureCategory.Commerce] = ["commercial.supermarket", "commercial.shopping_mall", "commercial.marketplace", "building.commercial"],
            [InfrastructureCategory.Leisure] = ["leisure.park", "leisure.playground", "activity.sport_club", "catering.cafe", "entertainment.cinema"]
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
        var boundedLimit = Math.Clamp(limit, 1, 20);
        var requestedCategories = categories.Count == 0
            ? Enum.GetValues<InfrastructureCategory>()
            : categories.Distinct().ToArray();

        var cacheKey = $"{ProviderName}:{Math.Round(latitude, 4)}:{Math.Round(longitude, 4)}:{boundedRadius}:{boundedLimit}:{string.Join('|', requestedCategories.Order().Select(static x => x.ToString()))}";
        if (memoryCache.TryGetValue<IReadOnlyCollection<ExternalPoiResult>>(cacheKey, out var cached) && cached is not null)
            return cached;

        var categoryTasks = requestedCategories
            .Select(async category =>
            {
                var geoapifyCategories = CategoryMap.TryGetValue(category, out var mapped)
                    ? mapped
                    : [];

                return await geoapifyPlacesClient.GetPlacesAsync(
                    latitude,
                    longitude,
                    boundedRadius,
                    geoapifyCategories,
                    boundedLimit,
                    cancellationToken);
            })
            .ToArray();

        var raw = (await Task.WhenAll(categoryTasks))
            .SelectMany(static x => x)
            .ToArray();
        var mapped = raw
            .Select(place => new ExternalPoiResult
            {
                ExternalId = place.ExternalId,
                Name = place.Name,
                AddressText = place.AddressText,
                Latitude = (decimal)place.Latitude,
                Longitude = (decimal)place.Longitude,
                DistanceMeters = CalculateDistanceMeters((decimal)latitude, (decimal)longitude, (decimal)place.Latitude, (decimal)place.Longitude),
                Source = ProviderName,
                RawCategory = place.Categories.FirstOrDefault(),
                RawCategories = place.Categories,
                Tags = place.Tags
            })
            .GroupBy(static x => $"{x.ExternalId ?? string.Empty}:{x.Name.Trim().ToLowerInvariant()}:{decimal.Round(x.Latitude, 5)}:{decimal.Round(x.Longitude, 5)}")
            .Select(static group => group.OrderBy(x => x.DistanceMeters).First())
            .OrderBy(static x => x.DistanceMeters)
            .ToArray();

        memoryCache.Set(cacheKey, mapped, TimeSpan.FromMinutes(scoringOptions.Value.CacheMinutes));
        logger.LogInformation(
            "Geoapify nearby search completed. Latitude: {Latitude}, Longitude: {Longitude}, Radius: {Radius}, Count: {Count}",
            latitude,
            longitude,
            boundedRadius,
            mapped.Length);

        return mapped;
    }

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
