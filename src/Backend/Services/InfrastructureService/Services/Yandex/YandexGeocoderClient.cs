using InfrastructureService.Services.Yandex.Abstractions;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace InfrastructureService.Services.Yandex;

public sealed class YandexGeocoderClient(HttpClient httpClient, IOptions<YandexOptions> options) : IYandexGeocoderClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly YandexOptions _options = options.Value;

    public async Task<YandexResolvedLocation?> ResolveAsync(string addressText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(addressText))
            return null;

        var requestUri = BuildRequestUri(Uri.EscapeDataString(addressText));

        return await ResolveCoreAsync(requestUri, addressText.Trim(), cancellationToken);
    }

    public async Task<YandexResolvedLocation?> ResolveAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken)
    {
        var coordinates = string.Create(
            CultureInfo.InvariantCulture,
            $"{longitude},{latitude}");
        var requestUri = BuildRequestUri(Uri.EscapeDataString(coordinates));

        return await ResolveCoreAsync(
            requestUri,
            string.Create(CultureInfo.InvariantCulture, $"{latitude:F6}, {longitude:F6}"),
            cancellationToken);
    }

    private async Task<YandexResolvedLocation?> ResolveCoreAsync(
        string requestUri,
        string fallbackAddressText,
        CancellationToken cancellationToken)
    {

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var feature = FindFirstFeature(document.RootElement);
        if (feature is null)
            return null;

        var point = feature.Value.GetProperty("Point");
        var coordinatesText = point.GetProperty("pos").GetString();
        if (string.IsNullOrWhiteSpace(coordinatesText))
            return null;

        var coordinates = coordinatesText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (coordinates.Length != 2 ||
            !decimal.TryParse(coordinates[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude) ||
            !decimal.TryParse(coordinates[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
        {
            return null;
        }

        var geoObject = feature.Value;
        var metaData = geoObject.GetProperty("metaDataProperty").GetProperty("GeocoderMetaData");
        var address = metaData.TryGetProperty("Address", out var addressElement) ? addressElement : default;

        string? country = null;
        string? region = null;
        string? city = null;
        string? street = null;
        string? houseNumber = null;

        if (address.ValueKind != JsonValueKind.Undefined &&
            address.TryGetProperty("Components", out var componentsElement) &&
            componentsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var component in componentsElement.EnumerateArray())
            {
                var kind = component.TryGetProperty("kind", out var kindElement)
                    ? kindElement.GetString()
                    : null;
                var value = component.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(value))
                    continue;

                switch (kind)
                {
                    case "country":
                        country = value;
                        break;
                    case "province":
                        region = value;
                        break;
                    case "locality":
                    case "area":
                        city ??= value;
                        break;
                    case "street":
                        street = value;
                        break;
                    case "house":
                        houseNumber = value;
                        break;
                }
            }
        }

        var resolvedAddress = metaData.TryGetProperty("text", out var textElement)
            ? textElement.GetString()
            : null;

        return new YandexResolvedLocation
        {
            AddressText = string.IsNullOrWhiteSpace(resolvedAddress) ? fallbackAddressText : resolvedAddress,
            NormalizedAddressText = NormalizeAddress(resolvedAddress ?? fallbackAddressText),
            Latitude = latitude,
            Longitude = longitude,
            Country = country,
            Region = region,
            City = city,
            Street = street,
            HouseNumber = houseNumber
        };
    }

    private string BuildRequestUri(string geocode)
        => $"1.x/?apikey={Uri.EscapeDataString(_options.ApiKey)}&format=json&lang={Uri.EscapeDataString(_options.Language)}&geocode={geocode}";

    private static string NormalizeAddress(string value)
        => value.Trim().ToLowerInvariant();

    private static JsonElement? FindFirstFeature(JsonElement root)
    {
        if (!root.TryGetProperty("response", out var response))
            return null;

        if (!response.TryGetProperty("GeoObjectCollection", out var collection))
            return null;

        if (!collection.TryGetProperty("featureMember", out var featureMembers) ||
            featureMembers.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var featureMember in featureMembers.EnumerateArray())
        {
            if (featureMember.TryGetProperty("GeoObject", out var geoObject))
                return geoObject;
        }

        return null;
    }
}
