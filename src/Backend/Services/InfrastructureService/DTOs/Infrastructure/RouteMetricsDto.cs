namespace InfrastructureService.DTOs.Infrastructure;

public sealed class RouteMetricsDto
{
    public decimal DistanceMeters { get; init; }
    public decimal? WalkMinutes { get; init; }
    public bool IsWalkMinutesEstimated { get; init; }
    public decimal? DriveMinutes { get; init; }
    public bool IsDriveMinutesEstimated { get; init; }
}
