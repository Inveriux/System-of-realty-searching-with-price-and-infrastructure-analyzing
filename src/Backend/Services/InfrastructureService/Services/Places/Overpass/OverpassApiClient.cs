using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using InfrastructureService.Services.Places.Overpass.Abstractions;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Places.Overpass;

public sealed class OverpassApiClient(
    HttpClient httpClient,
    IOptions<OverpassOptions> options,
    ILogger<OverpassApiClient> logger) : IOverpassApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly OverpassOptions _options = options.Value;

    public async Task<IReadOnlyCollection<OverpassElement>> QueryNearbyAsync(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        IReadOnlyCollection<OverpassTagFilter> filters,
        CancellationToken cancellationToken)
    {
        if (filters.Count == 0)
            return [];

        var query = BuildQuery(latitude, longitude, radiusMeters, filters);

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "interpreter")
            {
                Content = new StringContent(query, Encoding.UTF8, "text/plain")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var payload = await JsonSerializer.DeserializeAsync<OverpassResponse>(stream, JsonOptions, cancellationToken);

                return payload?.Elements?
                    .Where(static x => x is not null)
                    .Select(static x => new OverpassElement
                    {
                        Id = x!.Id,
                        Type = x.Type ?? string.Empty,
                        Latitude = x.Latitude ?? x.Center?.Latitude,
                        Longitude = x.Longitude ?? x.Center?.Longitude,
                        Tags = x.Tags ?? new Dictionary<string, string>()
                    })
                    .Where(static x => x.Latitude.HasValue && x.Longitude.HasValue && x.Tags.Count > 0)
                    .ToArray()
                    ?? [];
            }

            if (!IsTransient(response.StatusCode) || attempt == 2)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Overpass API request failed. Status: {StatusCode}. Response: {Response}",
                    (int)response.StatusCode,
                    Truncate(body, 1500));
                return [];
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
        }

        return [];
    }

    private string BuildQuery(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        IReadOnlyCollection<OverpassTagFilter> filters)
    {
        var builder = new StringBuilder();
        builder.Append($"[out:json][timeout:{_options.QueryTimeoutSeconds}];(");

        foreach (var filter in filters)
        {
            var values = string.Join("|", filter.Values.Select(EscapeRegexValue));
            var coords = string.Create(
                CultureInfo.InvariantCulture,
                $"{radiusMeters},{latitude},{longitude}");

            builder.Append($"node(around:{coords})[\"{filter.Key}\"~\"^({values})$\"];");
            builder.Append($"way(around:{coords})[\"{filter.Key}\"~\"^({values})$\"];");
            builder.Append($"relation(around:{coords})[\"{filter.Key}\"~\"^({values})$\"];");
        }

        builder.Append(");out center tags;");
        return builder.ToString();
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.RequestTimeout
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout
            or HttpStatusCode.InternalServerError;

    private static string EscapeRegexValue(string value)
        => value.Replace("_", "\\_").Replace(".", "\\.");

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..maxLength];
    }

    private sealed class OverpassResponse
    {
        public List<OverpassResponseElement?>? Elements { get; init; }
    }

    private sealed class OverpassResponseElement
    {
        public long Id { get; init; }
        public string? Type { get; init; }
        public decimal? Lat { get; init; }
        public decimal? Lon { get; init; }
        public OverpassCenter? Center { get; init; }
        public Dictionary<string, string>? Tags { get; init; }

        public decimal? Latitude => Lat;
        public decimal? Longitude => Lon;
    }

    private sealed class OverpassCenter
    {
        public decimal Lat { get; init; }
        public decimal Lon { get; init; }

        public decimal Latitude => Lat;
        public decimal Longitude => Lon;
    }
}
