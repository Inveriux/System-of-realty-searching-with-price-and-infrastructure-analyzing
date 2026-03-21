namespace InfrastructureService.Services.Places;

public sealed class InfrastructureScoringOptions
{
    public const string SectionName = "InfrastructureScoring";

    public int DefaultRadiusMeters { get; set; } = 1500;
    public int MaxRadiusMeters { get; set; } = 3000;
    public int MaxPoiCount { get; set; } = 100;
    public int CacheMinutes { get; set; } = 10;
}
