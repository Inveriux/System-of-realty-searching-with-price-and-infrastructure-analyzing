namespace InfrastructureService.Services.Places.Geoapify.Abstractions;

public sealed class GeoapifyPlaceFeature
{
    public string? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AddressText { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public IReadOnlyCollection<string> Categories { get; init; } = [];
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}
