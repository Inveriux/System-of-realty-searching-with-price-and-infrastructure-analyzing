namespace InfrastructureService.Services.Places.Abstractions;

public sealed class ExternalPoiResult
{
    public string? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AddressText { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal DistanceMeters { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? RawCategory { get; init; }
    public IReadOnlyCollection<string> RawCategories { get; init; } = [];
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}
