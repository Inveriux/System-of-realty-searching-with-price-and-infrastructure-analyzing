namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureConfigDto
{
    public IReadOnlyCollection<InfrastructureSubcategoryConfigDto> Categories { get; init; } = [];
    public Dictionary<string, string> ClassificationVersions { get; init; } = [];
}

public sealed class InfrastructureSubcategoryConfigDto
{
    public string CategoryCode { get; init; } = string.Empty;
    public string SubcategoryCode { get; init; } = string.Empty;
    public int PrimaryRadiusMeters { get; init; }
    public int? FallbackRadiusMeters { get; init; }
    public bool FallbackEnabled { get; init; }
}
