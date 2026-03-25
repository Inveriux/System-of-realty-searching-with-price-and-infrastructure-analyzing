namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureSummaryDto
{
    public Guid LocationId { get; init; }
    public string AddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public InfrastructureScoreDto Score { get; init; } = null!;
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Highlights { get; init; } = [];
    public IReadOnlyCollection<PoiDto> Pois { get; init; } = [];
    public IReadOnlyCollection<InfrastructureNearbyCategoryDto> NearbyCategories { get; init; } = [];
    public InfrastructureTransportDetailsDto TransportDetails { get; init; } = new();
    public InfrastructureSearchFeaturesDto SearchFeatures { get; init; } = new();
    public Dictionary<string, int> PrimaryRadiusConfig { get; init; } = [];
    public Dictionary<string, int?> FallbackRadiusConfig { get; init; } = [];
    public IReadOnlyCollection<string> MissingPrimaryCategories { get; init; } = [];
    public Dictionary<string, string> ClassificationVersions { get; init; } = [];
}
