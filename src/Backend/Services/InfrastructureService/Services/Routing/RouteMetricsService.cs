using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Services.Routing.Abstractions;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Routing;

public sealed class RouteMetricsService(IOptions<RouteMetricsOptions> options) : IRouteMetricsService
{
    public Task<RouteMetricsDto> GetMetricsAsync(
        decimal originLatitude,
        decimal originLongitude,
        decimal destinationLatitude,
        decimal destinationLongitude,
        CancellationToken cancellationToken)
    {
        var distanceMeters = CalculateDistanceMeters(originLatitude, originLongitude, destinationLatitude, destinationLongitude);
        var walkMinutes = EstimateMinutes(distanceMeters, options.Value.WalkSpeedKmPerHour);
        var driveMinutes = EstimateMinutes(distanceMeters, options.Value.DriveSpeedKmPerHour);

        return Task.FromResult(new RouteMetricsDto
        {
            DistanceMeters = distanceMeters,
            WalkMinutes = walkMinutes,
            IsWalkMinutesEstimated = true,
            DriveMinutes = driveMinutes,
            IsDriveMinutesEstimated = true
        });
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
