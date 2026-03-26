using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.ScoringEntities;
using System.Text.RegularExpressions;

namespace InfrastructureService.Services.Scoring;

public sealed partial class InfrastructureScoringInputBuilder : IInfrastructureScoringInputBuilder
{
    private static readonly string[] TransportSurfaceSubcategories = ["bus_stop", "tram_stop", "trolleybus_stop"];

    public InfrastructureScoringInput Build(LocationPoiSnapshot snapshot, InfrastructureDetailsDto details)
    {
        var result = new InfrastructureScoringInput();
        result.CategoryEntities["transport"] = BuildTransportEntities(snapshot, details.TransportDetails);
        var snapshotItems = snapshot.Items.ToArray();
        result.CategoryEntities["healthcare"] = BuildGenericEntities(snapshotItems, "healthcare", static cluster => new HealthcareScoringEntity(cluster.SubcategoryCode, cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount));
        result.CategoryEntities["commerce"] = BuildGenericEntities(snapshotItems, "commerce", static cluster => new CommerceScoringEntity(cluster.SubcategoryCode, cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount));
        result.CategoryEntities["education"] = BuildGenericEntities(snapshotItems, "education", static cluster => new EducationScoringEntity(cluster.SubcategoryCode, cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount));
        result.CategoryEntities["leisure"] = BuildGenericEntities(snapshotItems, "leisure", static cluster => new LeisureScoringEntity(cluster.SubcategoryCode, cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount));
        return result;
    }

    private static IReadOnlyCollection<InfrastructureScoringEntity> BuildTransportEntities(LocationPoiSnapshot snapshot, InfrastructureTransportDetailsDto transportDetails)
    {
        var entities = new List<InfrastructureScoringEntity>();
        foreach (var group in transportDetails.StopGroups)
        {
            foreach (var stopType in group.StopTypes.Where(static value => TransportSurfaceSubcategories.Contains(value, StringComparer.OrdinalIgnoreCase)))
            {
                entities.Add(new TransportScoringEntity(
                    $"{stopType}_group",
                    group.CenterLatitude,
                    group.CenterLongitude,
                    group.NearestDistanceMeters,
                    group.DisplayName,
                    Math.Max(group.PlatformCount, group.PointCount),
                    stopType));
            }
        }

        var transportClusters = DeduplicateItems(
            snapshot.Items.Where(static item => item.Poi?.Category?.Name.Equals("transport", StringComparison.OrdinalIgnoreCase) == true),
            includeSurfaceStops: false);

        foreach (var cluster in transportClusters)
        {
            var entity = cluster.SubcategoryCode switch
            {
                "metro_station" => new TransportScoringEntity("metro_station", cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount, "metro_station"),
                "railway_station" => new TransportScoringEntity("railway_station", cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount, "railway_station"),
                "parking" => new TransportScoringEntity("parking", cluster.Latitude, cluster.Longitude, cluster.DistanceMeters, cluster.Name, cluster.SourceCount, "parking"),
                _ => null
            };

            if (entity is not null)
            {
                entities.Add(entity);
            }
        }

        return entities
            .Where(static entity => entity is not null)
            .OrderBy(static entity => entity.DistanceMeters)
            .ToArray();
    }

    private static IReadOnlyCollection<InfrastructureScoringEntity> BuildGenericEntities(
        IReadOnlyCollection<LocationPoiSnapshotItem> items,
        string categoryCode,
        Func<DeduplicatedCluster, InfrastructureScoringEntity> factory)
        => DeduplicateItems(
                items.Where(item => item.Poi?.Category?.Name.Equals(categoryCode, StringComparison.OrdinalIgnoreCase) == true),
                includeSurfaceStops: true)
            .Select(factory)
            .OrderBy(static entity => entity.DistanceMeters)
            .ToArray();

    private static IReadOnlyCollection<DeduplicatedCluster> DeduplicateItems(IEnumerable<LocationPoiSnapshotItem> items, bool includeSurfaceStops)
    {
        var filtered = items
            .Where(static item => item.Poi is not null && !string.IsNullOrWhiteSpace(item.Poi.SubcategoryCode))
            .Where(item => includeSurfaceStops || !TransportSurfaceSubcategories.Contains(item.Poi!.SubcategoryCode!, StringComparer.OrdinalIgnoreCase))
            .OrderBy(static item => item.DistanceMeters)
            .ToArray();

        var clusters = new List<List<LocationPoiSnapshotItem>>();
        foreach (var item in filtered)
        {
            var existing = clusters.FirstOrDefault(cluster => BelongsToCluster(cluster, item));
            if (existing is null)
            {
                clusters.Add([item]);
                continue;
            }

            existing.Add(item);
        }

        return clusters.Select(BuildCluster).ToArray();
    }

    private static bool BelongsToCluster(IReadOnlyCollection<LocationPoiSnapshotItem> cluster, LocationPoiSnapshotItem candidate)
    {
        var clusterItem = cluster.First();
        var candidatePoi = candidate.Poi!;
        var clusterPoi = clusterItem.Poi!;
        if (!string.Equals(clusterPoi.SubcategoryCode, candidatePoi.SubcategoryCode, StringComparison.OrdinalIgnoreCase))
            return false;

        var sameRoundedCoordinate =
            decimal.Round(clusterPoi.Latitude, 4) == decimal.Round(candidatePoi.Latitude, 4) &&
            decimal.Round(clusterPoi.Longitude, 4) == decimal.Round(candidatePoi.Longitude, 4);
        if (sameRoundedCoordinate)
            return true;

        var distance = CalculateDistanceMeters(clusterPoi.Latitude, clusterPoi.Longitude, candidatePoi.Latitude, candidatePoi.Longitude);
        if (distance > 35m)
            return false;

        var clusterName = NormalizeName(clusterPoi.Name);
        var candidateName = NormalizeName(candidatePoi.Name);
        return string.IsNullOrWhiteSpace(clusterName)
               || string.IsNullOrWhiteSpace(candidateName)
               || string.Equals(clusterName, candidateName, StringComparison.Ordinal);
    }

    private static DeduplicatedCluster BuildCluster(IReadOnlyCollection<LocationPoiSnapshotItem> cluster)
    {
        var representative = cluster.OrderBy(static item => item.DistanceMeters).First();
        return new DeduplicatedCluster(
            representative.Poi!.SubcategoryCode!,
            representative.Poi.Latitude,
            representative.Poi.Longitude,
            representative.DistanceMeters,
            representative.Poi.Name,
            cluster.Count);
    }

    private static string NormalizeName(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : MultiWhitespaceRegex().Replace(value.Trim().ToLowerInvariant(), " ");

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
        return (decimal)(earthRadiusMeters * c);
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultiWhitespaceRegex();

    private sealed record DeduplicatedCluster(
        string SubcategoryCode,
        decimal Latitude,
        decimal Longitude,
        decimal DistanceMeters,
        string? Name,
        int SourceCount);
}
