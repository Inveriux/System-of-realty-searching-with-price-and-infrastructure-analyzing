namespace InfrastructureService.Services.Routing.Abstractions;

public interface IRouteServiceClient
{
    Task<NearestPointResult> GetNearestAsync(decimal latitude, decimal longitude, string profile, CancellationToken cancellationToken);
    Task<RouteResult?> GetRouteAsync(RouteCoordinate origin, RouteCoordinate destination, string profile, CancellationToken cancellationToken);
    Task<RouteMatrixResult?> GetTableAsync(RouteCoordinate origin, IReadOnlyCollection<RouteCoordinate> destinations, string profile, CancellationToken cancellationToken);
}
