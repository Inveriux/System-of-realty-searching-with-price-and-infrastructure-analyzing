using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Abstractions;
using System.Text.Json;

namespace InfrastructureService.Services.Scoring;

public sealed class InfrastructureScoringService : IInfrastructureScoringService
{
    public InfrastructureScore Calculate(Location location, LocationPoiSnapshot snapshot)
    {
        var grouped = snapshot.Items
            .Where(x => x.Poi?.Category is not null)
            .GroupBy(x => x.Poi!.Category!.Name)
            .ToDictionary(
                x => x.Key,
                x => Math.Round(x.Sum(item => Math.Max(0.1m, item.Poi!.Category!.Weight) / Math.Max(1m, item.DistanceMeters / 100m)), 4));

        var education = GetFeature(grouped, "education");
        var healthcare = GetFeature(grouped, "healthcare");
        var transport = GetFeature(grouped, "transport");
        var commerce = GetFeature(grouped, "commerce");
        var leisure = GetFeature(grouped, "leisure");
        var total = education + healthcare + transport + commerce + leisure;

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
            SearchFeaturesJson = JsonSerializer.Serialize(grouped)
        };
    }

    private static decimal GetFeature(IReadOnlyDictionary<string, decimal> grouped, string name)
        => grouped.TryGetValue(name, out var value) ? value : 0m;
}
