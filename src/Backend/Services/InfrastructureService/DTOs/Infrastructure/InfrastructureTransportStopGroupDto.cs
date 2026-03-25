namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureTransportStopGroupDto
{
    public string GroupId { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public decimal CenterLatitude { get; init; }
    public decimal CenterLongitude { get; init; }
    public decimal MaxDistanceWithinGroup { get; init; }
    public int PointCount { get; init; }
    public int PlatformCount { get; init; }
    public IReadOnlyCollection<string> StopTypes { get; init; } = [];
    public IReadOnlyCollection<Guid> SourcePoiIds { get; init; } = [];
    public decimal NearestDistanceMeters { get; init; }
}
