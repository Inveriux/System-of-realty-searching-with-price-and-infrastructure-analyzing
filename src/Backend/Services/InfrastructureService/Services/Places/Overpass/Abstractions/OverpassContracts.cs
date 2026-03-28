namespace InfrastructureService.Services.Places.Overpass.Abstractions;

public sealed class OverpassTagFilter
{
    public string Key { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Values { get; init; } = [];
}

public sealed class OverpassResponseDto
{
    public decimal? Version { get; init; }
    public string? Generator { get; init; }
    public string? Remark { get; init; }
    public IReadOnlyCollection<OverpassResponseElementDto> Elements { get; init; } = [];
}

public sealed class OverpassResponseElementDto
{
    public string Type { get; init; } = string.Empty;
    public long Id { get; init; }
    public decimal? Lat { get; init; }
    public decimal? Lon { get; init; }
    public OverpassCenterDto? Center { get; init; }
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}

public sealed class OverpassCenterDto
{
    public decimal Lat { get; init; }
    public decimal Lon { get; init; }
}

public sealed class OverpassElement
{
    public long Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}
