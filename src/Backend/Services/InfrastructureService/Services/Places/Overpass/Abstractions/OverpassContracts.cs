namespace InfrastructureService.Services.Places.Overpass.Abstractions;

public sealed class OverpassTagFilter
{
    public string Key { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Values { get; init; } = [];
}

public sealed class OverpassElement
{
    public long Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}
