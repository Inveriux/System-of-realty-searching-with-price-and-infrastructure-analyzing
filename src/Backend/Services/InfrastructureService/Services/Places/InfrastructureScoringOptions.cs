using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Places;

public sealed class InfrastructureScoringOptions
{
    public const string SectionName = "InfrastructureScoring";

    public int DefaultRadiusMeters { get; set; } = 1200;
    public int MaxRadiusMeters { get; set; } = 15000;
    public int MaxPoiCount { get; set; } = 5000;
    public int CacheMinutes { get; set; } = 10;
    public string ActiveProfile { get; set; } = "residential";
    public IReadOnlyCollection<string> MetroApplicableCities { get; set; } = ["минск", "minsk"];
    public InfrastructureScoringProfileOptions ResidentialProfile { get; set; } = new();

    public InfrastructureScoringProfileOptions GetActiveProfile()
        => ResidentialProfile;
}
