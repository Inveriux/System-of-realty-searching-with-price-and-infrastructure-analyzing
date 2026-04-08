using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Services.Routing.Abstractions;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Routing;

public sealed class RouteMetricsService(
    IOptions<RouteMetricsOptions> options,
    IOptions<OsrmOptions> osrmOptions,
    IRouteServiceClient routeServiceClient,
    ILogger<RouteMetricsService> logger) : IRouteMetricsService
{
    public Task<RouteMetricsDto> GetMetricsAsync(
        decimal originLatitude,
        decimal originLongitude,
        decimal destinationLatitude,
        decimal destinationLongitude,
        CancellationToken cancellationToken)
        => GetMetricsInternalAsync(
            new RouteCoordinate(originLatitude, originLongitude),
            new RouteCoordinate(destinationLatitude, destinationLongitude),
            cancellationToken);

    public async Task<IReadOnlyList<RouteMetricsDto>> GetWalkingMetricsAsync(
        decimal originLatitude,
        decimal originLongitude,
        IReadOnlyCollection<RouteCoordinate> destinations,
        CancellationToken cancellationToken)
    {
        if (destinations.Count == 0)
        {
            return [];
        }

        var origin = new RouteCoordinate(originLatitude, originLongitude);
        if (!options.Value.Provider.Equals("Osrm", StringComparison.OrdinalIgnoreCase))
        {
            return destinations
                .Select(destination => BuildFallback(origin, destination, includeDrive: false))
                .ToArray();
        }

        try
        {
            var matrix = await routeServiceClient.GetTableAsync(origin, destinations, osrmOptions.Value.FootProfile, cancellationToken);
            if (matrix is null)
            {
                return destinations
                    .Select(destination => BuildFallback(origin, destination, includeDrive: false, fallbackMode: "straight_line"))
                    .ToArray();
            }

            var results = destinations
                .Select(static (_, index) => (RouteMetricsDto?)null)
                .ToArray();

            foreach (var entry in matrix.Entries)
            {
                var distanceMeters = decimal.Round(entry.DistanceMeters, 2);
                var walkMinutes = decimal.Round(entry.DurationSeconds / 60m, 1);
                results[entry.DestinationIndex] = new RouteMetricsDto
                {
                    DistanceMeters = distanceMeters,
                    RouteDistanceMeters = distanceMeters,
                    TravelDurationSeconds = decimal.Round(entry.DurationSeconds, 1),
                    WalkingMinutes = walkMinutes,
                    WalkMinutes = walkMinutes,
                    IsWalkMinutesEstimated = false,
                    UsedRouting = true
                };
            }

            for (var index = 0; index < results.Length; index++)
            {
                if (results[index] is null)
                {
                    results[index] = BuildFallback(origin, destinations.ElementAt(index), includeDrive: false, fallbackMode: "straight_line");
                }
            }

            return results!;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogWarning(ex, "OSRM walking matrix failed. Falling back to straight-line calculation.");
            return destinations
                .Select(destination => BuildFallback(origin, destination, includeDrive: false, fallbackMode: "straight_line"))
                .ToArray();
        }
    }

    private async Task<RouteMetricsDto> GetMetricsInternalAsync(
        RouteCoordinate origin,
        RouteCoordinate destination,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Provider.Equals("Osrm", StringComparison.OrdinalIgnoreCase))
        {
            return BuildFallback(origin, destination, includeDrive: true);
        }

        try
        {
            var walkingRoute = await routeServiceClient.GetRouteAsync(origin, destination, osrmOptions.Value.FootProfile, cancellationToken);
            var drivingRoute = await routeServiceClient.GetRouteAsync(origin, destination, osrmOptions.Value.CarProfile, cancellationToken);
            if (walkingRoute is null && drivingRoute is null)
            {
                return BuildFallback(origin, destination, includeDrive: true, fallbackMode: "straight_line");
            }

            var fallbackDistance = CalculateDistanceMeters(origin.Latitude, origin.Longitude, destination.Latitude, destination.Longitude);
            return new RouteMetricsDto
            {
                DistanceMeters = decimal.Round(walkingRoute?.DistanceMeters ?? drivingRoute?.DistanceMeters ?? fallbackDistance, 2),
                RouteDistanceMeters = decimal.Round(walkingRoute?.DistanceMeters ?? drivingRoute?.DistanceMeters ?? fallbackDistance, 2),
                TravelDurationSeconds = decimal.Round(walkingRoute?.DurationSeconds ?? drivingRoute?.DurationSeconds ?? 0m, 1),
                WalkingMinutes = walkingRoute is null ? EstimateMinutes(fallbackDistance, options.Value.WalkSpeedKmPerHour) : decimal.Round(walkingRoute.DurationSeconds / 60m, 1),
                WalkMinutes = walkingRoute is null ? EstimateMinutes(fallbackDistance, options.Value.WalkSpeedKmPerHour) : decimal.Round(walkingRoute.DurationSeconds / 60m, 1),
                IsWalkMinutesEstimated = walkingRoute is null,
                DriveMinutes = drivingRoute is null ? EstimateMinutes(fallbackDistance, options.Value.DriveSpeedKmPerHour) : decimal.Round(drivingRoute.DurationSeconds / 60m, 1),
                IsDriveMinutesEstimated = drivingRoute is null,
                UsedRouting = walkingRoute is not null || drivingRoute is not null,
                FallbackMode = walkingRoute is null || drivingRoute is null ? "straight_line" : "none"
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogWarning(ex, "OSRM route metrics failed. Falling back to straight-line calculation.");
            return BuildFallback(origin, destination, includeDrive: true, fallbackMode: "straight_line");
        }
    }

    private RouteMetricsDto BuildFallback(RouteCoordinate origin, RouteCoordinate destination, bool includeDrive, string fallbackMode = "none")
    {
        var distanceMeters = CalculateDistanceMeters(origin.Latitude, origin.Longitude, destination.Latitude, destination.Longitude);
        var walkMinutes = EstimateMinutes(distanceMeters, options.Value.WalkSpeedKmPerHour);
        var driveMinutes = EstimateMinutes(distanceMeters, options.Value.DriveSpeedKmPerHour);

        return new RouteMetricsDto
        {
            DistanceMeters = distanceMeters,
            RouteDistanceMeters = distanceMeters,
            TravelDurationSeconds = decimal.Round(walkMinutes * 60m, 1),
            WalkingMinutes = walkMinutes,
            WalkMinutes = walkMinutes,
            IsWalkMinutesEstimated = true,
            DriveMinutes = includeDrive ? driveMinutes : null,
            IsDriveMinutesEstimated = includeDrive,
            UsedRouting = false,
            FallbackMode = fallbackMode
        };
    }

    private static decimal EstimateMinutes(decimal distanceMeters, decimal speedKmPerHour)
    {
        if (speedKmPerHour <= 0)
            return 0;

        var hours = (distanceMeters / 1000m) / speedKmPerHour;
        return decimal.Round(hours * 60m, 1);
    }

    private static decimal CalculateDistanceMeters(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double earthRadiusMeters = 6_371_000d;
        var lat1 = DegreesToRadians((double)latitude1);
        var lon1 = DegreesToRadians((double)longitude1);
        var lat2 = DegreesToRadians((double)latitude2);
        var lon2 = DegreesToRadians((double)longitude2);
        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;
        var a = Math.Pow(Math.Sin(dLat / 2d), 2d)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2d), 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return decimal.Round((decimal)(earthRadiusMeters * c), 2);
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;
}
