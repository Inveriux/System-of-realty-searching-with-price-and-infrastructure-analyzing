using System.Globalization;
using System.Net;
using System.Text.Json;
using InfrastructureService.Services.Places.Geoapify.Abstractions;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Places.Geoapify;

public sealed class GeoapifyPlacesClient(
    HttpClient httpClient,
    IOptions<GeoapifyOptions> options,
    ILogger<GeoapifyPlacesClient> logger) : IGeoapifyPlacesClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly GeoapifyOptions _options = options.Value;

    public async Task<IReadOnlyCollection<GeoapifyPlaceFeature>> GetPlacesAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<string> categories,
        int limit,
        CancellationToken cancellationToken)
    {
        if (categories.Count == 0)
            return [];

        var requestUri = BuildRequestUri(latitude, longitude, radiusMeters, categories, limit);

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<GeoapifyPlacesResponse>(stream, JsonOptions, cancellationToken);
            return payload?.Features?
                .Where(static x => x.Properties is not null)
                .Select(static x =>
                {
                    var properties = x.Properties!;
                    var tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrWhiteSpace(properties.Category))
                        tags["category"] = properties.Category!;

                    return new GeoapifyPlaceFeature
                    {
                        ExternalId = properties.PlaceId,
                        Name = properties.GetNameOrFallback(),
                        AddressText = properties.Formatted,
                        Latitude = properties.Latitude,
                        Longitude = properties.Longitude,
                        Categories = properties.Categories ?? [],
                        Tags = tags
                    };
                })
                .ToArray()
                ?? [];
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogWarning(
            "Geoapify Places API request failed. Status: {StatusCode}. Response: {Response}",
            (int)response.StatusCode,
            Truncate(body, 1500));

        if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway)
            return [];

        return [];
    }

    private string BuildRequestUri(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<string> categories,
        int limit)
    {
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var categoryList = string.Join(",", categories);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"v2/places?categories={Uri.EscapeDataString(categoryList)}&filter=circle:{lon},{lat},{radiusMeters}&bias=proximity:{lon},{lat}&limit={limit}&lang={Uri.EscapeDataString(_options.Language)}&apiKey={Uri.EscapeDataString(_options.ApiKey)}");
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..maxLength];
    }

    private sealed class GeoapifyPlacesResponse
    {
        public List<GeoapifyPlaceItem>? Features { get; init; }
    }

    private sealed class GeoapifyPlaceItem
    {
        public GeoapifyProperties? Properties { get; init; }
    }

    private sealed class GeoapifyProperties
    {
        public string? PlaceId { get; init; }
        public JsonElement? Name { get; init; }
        public string? Formatted { get; init; }
        public double Lat { get; init; }
        public double Lon { get; init; }
        public string? Category { get; init; }
        public List<string>? Categories { get; init; }

        public double Latitude => Lat;
        public double Longitude => Lon;

        public string GetNameOrFallback()
        {
            if (Name is { } name)
            {
                if (name.ValueKind == JsonValueKind.String)
                    return name.GetString() ?? Formatted ?? "POI";

                if (name.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                    return name.ToString();
            }

            return Formatted ?? "POI";
        }
    }
}
