using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Places;

public sealed class InfrastructureScoringOptions
{
    public const string SectionName = "InfrastructureScoring";

    public int DefaultRadiusMeters { get; set; } = 1500;
    public int MaxRadiusMeters { get; set; } = 3000;
    public int MaxPoiCount { get; set; } = 100;
    public int CacheMinutes { get; set; } = 10;
    public string ActiveProfile { get; set; } = "residential";
    public IReadOnlyCollection<string> MetroApplicableCities { get; set; } = ["минск", "minsk"];
    public InfrastructureScoringProfileOptions ResidentialProfile { get; set; } = new();

    public InfrastructureScoringProfileOptions GetActiveProfile()
        => ResidentialProfile;
}
