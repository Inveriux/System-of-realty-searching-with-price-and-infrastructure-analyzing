namespace InfrastructureService.Services.Yandex.Abstractions;

public sealed class YandexResolvedLocation
{
    public string AddressText { get; init; } = string.Empty;
    public string NormalizedAddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? City { get; init; }
    public string? Street { get; init; }
    public string? HouseNumber { get; init; }
}

public sealed class YandexOrganizationSearchResult
{
    public string? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AddressText { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal DistanceMeters { get; init; }
}
