namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureSummaryDto
{
    public Guid LocationId { get; init; }
    public string AddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public InfrastructureScoreDto Score { get; init; } = null!;
    public IReadOnlyCollection<PoiDto> Pois { get; init; } = [];
}
