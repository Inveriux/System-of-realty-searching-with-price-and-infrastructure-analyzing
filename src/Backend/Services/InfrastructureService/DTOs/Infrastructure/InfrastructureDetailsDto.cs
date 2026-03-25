namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureDetailsDto
{
    public IReadOnlyCollection<InfrastructureNearbyCategoryDto> NearbyCategories { get; init; } = [];
    public InfrastructureTransportDetailsDto TransportDetails { get; init; } = new();
    public InfrastructureSearchFeaturesDto SearchFeatures { get; init; } = new();
    public Dictionary<string, int> PrimaryRadiusConfig { get; init; } = [];
    public Dictionary<string, int?> FallbackRadiusConfig { get; init; } = [];
    public IReadOnlyCollection<string> Highlights { get; init; } = [];
    public IReadOnlyCollection<string> MissingPrimaryCategories { get; init; } = [];
    public Dictionary<string, string> ClassificationVersions { get; init; } = [];
}
