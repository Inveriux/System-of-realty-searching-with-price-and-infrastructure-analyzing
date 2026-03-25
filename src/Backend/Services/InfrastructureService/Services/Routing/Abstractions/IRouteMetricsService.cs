using InfrastructureService.DTOs.Infrastructure;

namespace InfrastructureService.Services.Routing.Abstractions;

public interface IRouteMetricsService
{
    Task<RouteMetricsDto> GetMetricsAsync(
        decimal originLatitude,
        decimal originLongitude,
        decimal destinationLatitude,
        decimal destinationLongitude,
        CancellationToken cancellationToken);
}
