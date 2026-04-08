namespace InfrastructureService.Services.Routing;

public sealed record RouteCoordinate(decimal Latitude, decimal Longitude);

public sealed record NearestPointResult(
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters);

public sealed record RouteResult(
    decimal DistanceMeters,
    decimal DurationSeconds);

public sealed record RouteMatrixEntry(
    int DestinationIndex,
    decimal DistanceMeters,
    decimal DurationSeconds);

public sealed record RouteMatrixResult(
    IReadOnlyCollection<RouteMatrixEntry> Entries);
