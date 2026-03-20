namespace InfrastructureService.DTOs.Locations;

public sealed class ResolveLocationRequestDto
{
    public string? AddressText { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
}
