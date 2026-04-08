using System.Text.Json;
using InfrastructureService.Services.Routing.Abstractions;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Routing;

public sealed class OsrmRouteServiceClient(
    HttpClient httpClient,
    IOptions<OsrmOptions> options,
    ILogger<OsrmRouteServiceClient> logger) : IRouteServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly OsrmOptions _options = options.Value;

    public async Task<NearestPointResult> GetNearestAsync(decimal latitude, decimal longitude, string profile, CancellationToken cancellationToken)
    {
        var path = $"/nearest/v1/{profile}/{ToLonLat(longitude, latitude)}";
        using var response = await httpClient.GetAsync(BuildUri(profile, path), cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<NearestResponseDto>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("OSRM nearest response is empty.");
        var waypoint = payload.Waypoints.FirstOrDefault()
            ?? throw new InvalidOperationException("OSRM nearest response does not contain waypoints.");

        return new NearestPointResult(
            Latitude: waypoint.Location[1],
            Longitude: waypoint.Location[0],
            DistanceMeters: waypoint.Distance);
    }

    public async Task<RouteResult?> GetRouteAsync(RouteCoordinate origin, RouteCoordinate destination, string profile, CancellationToken cancellationToken)
    {
        var path = $"/route/v1/{profile}/{ToLonLat(origin.Longitude, origin.Latitude)};{ToLonLat(destination.Longitude, destination.Latitude)}?overview=false";

        try
        {
            using var response = await httpClient.GetAsync(BuildUri(profile, path), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OSRM route request failed. Status: {StatusCode}. Path: {Path}", response.StatusCode, path);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<RouteResponseDto>(stream, JsonOptions, cancellationToken);
            var route = payload?.Routes.FirstOrDefault();
            return route is null ? null : new RouteResult(route.Distance, route.Duration);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "OSRM route request failed for profile {Profile}.", profile);
            return null;
        }
    }

    public async Task<RouteMatrixResult?> GetTableAsync(RouteCoordinate origin, IReadOnlyCollection<RouteCoordinate> destinations, string profile, CancellationToken cancellationToken)
    {
        if (destinations.Count == 0)
        {
            return new RouteMatrixResult([]);
        }

        var coordinates = string.Join(
            ';',
            new[] { origin }.Concat(destinations).Select(static x => ToLonLat(x.Longitude, x.Latitude)));
        var destinationIndexes = string.Join(';', Enumerable.Range(1, destinations.Count));
        var path = $"/table/v1/{profile}/{coordinates}?sources=0&destinations={destinationIndexes}&annotations=duration,distance";

        try
        {
            using var response = await httpClient.GetAsync(BuildUri(profile, path), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OSRM table request failed. Status: {StatusCode}. Path: {Path}", response.StatusCode, path);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<TableResponseDto>(stream, JsonOptions, cancellationToken);
            if (payload?.Durations is null || payload.Distances is null || payload.Durations.Count == 0 || payload.Distances.Count == 0)
            {
                return null;
            }

            var durationRow = payload.Durations[0];
            var distanceRow = payload.Distances[0];
            var entries = new List<RouteMatrixEntry>(destinations.Count);
            for (var index = 0; index < destinations.Count; index++)
            {
                var duration = durationRow.ElementAtOrDefault(index);
                var distance = distanceRow.ElementAtOrDefault(index);
                if (duration is null || distance is null)
                {
                    continue;
                }

                entries.Add(new RouteMatrixEntry(index, distance.Value, duration.Value));
            }

            return new RouteMatrixResult(entries);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "OSRM table request failed for profile {Profile}.", profile);
            return null;
        }
    }

    private static string ToLonLat(decimal longitude, decimal latitude)
        => $"{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

    private string BuildUri(string profile, string path)
    {
        var baseUrl = profile.Equals(_options.CarProfile, StringComparison.OrdinalIgnoreCase)
            ? _options.CarBaseUrl ?? _options.BaseUrl
            : profile.Equals(_options.FootProfile, StringComparison.OrdinalIgnoreCase)
                ? _options.FootBaseUrl ?? _options.BaseUrl
                : _options.BaseUrl;

        return $"{baseUrl.TrimEnd('/')}{path}";
    }

    private sealed class NearestResponseDto
    {
        public List<WaypointDto> Waypoints { get; init; } = [];
    }

    private sealed class RouteResponseDto
    {
        public List<RouteDto> Routes { get; init; } = [];
    }

    private sealed class TableResponseDto
    {
        public List<List<decimal?>> Distances { get; init; } = [];
        public List<List<decimal?>> Durations { get; init; } = [];
    }

    private sealed class WaypointDto
    {
        public decimal Distance { get; init; }
        public decimal[] Location { get; init; } = [];
    }

    private sealed class RouteDto
    {
        public decimal Distance { get; init; }
        public decimal Duration { get; init; }
    }
}
