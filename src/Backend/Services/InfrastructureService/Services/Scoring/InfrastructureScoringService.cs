using InfrastructureService.Models;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Scoring.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace InfrastructureService.Services.Scoring;

public sealed class InfrastructureScoringService(IOptions<InfrastructureScoringOptions> options) : IInfrastructureScoringService
{
    private static readonly IReadOnlyDictionary<string, decimal> CategoryWeights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["education"] = 1.10m,
        ["healthcare"] = 1.05m,
        ["transport"] = 1.20m,
        ["commerce"] = 0.95m,
        ["leisure"] = 0.85m
    };

    private static readonly IReadOnlyDictionary<string, decimal> TargetCounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["education"] = 5m,
        ["healthcare"] = 6m,
        ["transport"] = 8m,
        ["commerce"] = 10m,
        ["leisure"] = 7m
    };

    public InfrastructureScore Calculate(Location location, LocationPoiSnapshot snapshot)
    {
        var grouped = snapshot.Items
            .Where(x => x.Poi?.Category is not null)
            .GroupBy(x => x.Poi!.Category!.Name)
            .ToDictionary(static x => x.Key, x => x.OrderBy(static item => item.DistanceMeters).ToArray(), StringComparer.OrdinalIgnoreCase);

        var education = CalculateCategoryScore(grouped, "education");
        var healthcare = CalculateCategoryScore(grouped, "healthcare");
        var transport = CalculateCategoryScore(grouped, "transport");
        var commerce = CalculateCategoryScore(grouped, "commerce");
        var leisure = CalculateCategoryScore(grouped, "leisure");
        var total = CalculateTotalScore(education, healthcare, transport, commerce, leisure);

        var searchFeatures = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["education_score"] = education,
            ["healthcare_score"] = healthcare,
            ["transport_score"] = transport,
            ["commerce_score"] = commerce,
            ["leisure_score"] = leisure
        };

        foreach (var (category, items) in grouped)
        {
            searchFeatures[$"{category}_count"] = items.Length;
            searchFeatures[$"{category}_nearest_m"] = items.Length == 0 ? 0m : items[0].DistanceMeters;
        }

        return new InfrastructureScore
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SnapshotId = snapshot.Id,
            TotalScore = total,
            EducationScore = education,
            HealthcareScore = healthcare,
            TransportScore = transport,
            CommerceScore = commerce,
            LeisureScore = leisure,
            PoiCount = snapshot.Items.Count,
            CalculatedAtUtc = DateTime.UtcNow,
            SearchFeaturesJson = JsonSerializer.Serialize(searchFeatures)
        };
    }

    private decimal CalculateCategoryScore(
        IReadOnlyDictionary<string, LocationPoiSnapshotItem[]> grouped,
        string category)
    {
        if (!grouped.TryGetValue(category, out var items) || items.Length == 0)
            return 0m;

        var radiusMeters = Math.Max(100m, options.Value.DefaultRadiusMeters);
        var nearest = items[0].DistanceMeters;
        var nearestScore = Math.Clamp((radiusMeters - nearest) / radiusMeters, 0m, 1m) * 60m;

        var targetCount = TargetCounts.TryGetValue(category, out var countTarget) ? countTarget : 6m;
        var densityScore = Math.Min(items.Length / targetCount, 1m) * 40m;

        return Math.Round(Math.Min(100m, nearestScore + densityScore), 2);
    }

    private static decimal CalculateTotalScore(
        decimal education,
        decimal healthcare,
        decimal transport,
        decimal commerce,
        decimal leisure)
    {
        var weightedSum =
            education * CategoryWeights["education"] +
            healthcare * CategoryWeights["healthcare"] +
            transport * CategoryWeights["transport"] +
            commerce * CategoryWeights["commerce"] +
            leisure * CategoryWeights["leisure"];

        var totalWeight = CategoryWeights.Values.Sum();
        return Math.Round(weightedSum / totalWeight, 2);
    }
}
