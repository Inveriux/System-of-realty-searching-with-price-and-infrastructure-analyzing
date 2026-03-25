namespace InfrastructureService.Services.Routing;

public sealed class RouteMetricsOptions
{
    public const string SectionName = "RouteMetrics";

    public string Provider { get; set; } = "Estimated";
    public decimal WalkSpeedKmPerHour { get; set; } = 4.8m;
    public decimal DriveSpeedKmPerHour { get; set; } = 28m;
}
