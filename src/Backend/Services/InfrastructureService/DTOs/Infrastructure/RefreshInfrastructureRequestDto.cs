namespace InfrastructureService.DTOs.Infrastructure;

public sealed class RefreshInfrastructureRequestDto
{
    public Guid? LocationId { get; init; }
    public string? AddressText { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public int SearchRadiusMeters { get; init; } = 1500;
}
