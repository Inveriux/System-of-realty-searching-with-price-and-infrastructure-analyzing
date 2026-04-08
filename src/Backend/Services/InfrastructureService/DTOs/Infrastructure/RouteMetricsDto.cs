namespace InfrastructureService.DTOs.Infrastructure;

public sealed class RouteMetricsDto
{
    public decimal DistanceMeters { get; init; }
    public decimal? RouteDistanceMeters { get; init; }
    public decimal? TravelDurationSeconds { get; init; }
    public decimal? WalkingMinutes { get; init; }
    public decimal? WalkMinutes { get; init; }
    public bool IsWalkMinutesEstimated { get; init; }
    public decimal? DriveMinutes { get; init; }
    public bool IsDriveMinutesEstimated { get; init; }
    public bool UsedRouting { get; init; }
    public string FallbackMode { get; init; } = "none";
}
