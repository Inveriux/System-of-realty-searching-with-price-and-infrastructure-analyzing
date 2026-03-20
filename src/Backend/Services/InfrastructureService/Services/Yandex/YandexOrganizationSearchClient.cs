using InfrastructureService.Services.Yandex.Abstractions;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace InfrastructureService.Services.Yandex;

public sealed class YandexOrganizationSearchClient(HttpClient httpClient, IOptions<YandexOptions> options) : IYandexOrganizationSearchClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly YandexOptions _options = options.Value;

    public async Task<IReadOnlyCollection<YandexOrganizationSearchResult>> SearchAsync(
        decimal latitude,
        decimal longitude,
        string searchText,
        int radiusMeters,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"v1/?apikey={Uri.EscapeDataString(_options.ApiKey)}&lang={Uri.EscapeDataString(_options.Language)}&text={Uri.EscapeDataString(searchText)}&ll={longitude.ToString(CultureInfo.InvariantCulture)},{latitude.ToString(CultureInfo.InvariantCulture)}&spn={BuildSpn(radiusMeters)}&type=biz&results=20";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return [];

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("features", out var featuresElement) ||
            featuresElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var items = new List<YandexOrganizationSearchResult>();

        foreach (var feature in featuresElement.EnumerateArray())
        {
            if (!feature.TryGetProperty("geometry", out var geometry) ||
                !geometry.TryGetProperty("coordinates", out var coordinates) ||
                coordinates.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var coordinateValues = coordinates.EnumerateArray().ToArray();
            if (coordinateValues.Length != 2 ||
                !coordinateValues[0].TryGetDecimal(out var poiLongitude) ||
                !coordinateValues[1].TryGetDecimal(out var poiLatitude))
            {
                continue;
            }

            var properties = feature.TryGetProperty("properties", out var propertiesElement)
                ? propertiesElement
                : default;
            var companyMetaData = properties.ValueKind != JsonValueKind.Undefined &&
                                  properties.TryGetProperty("CompanyMetaData", out var companyElement)
                ? companyElement
                : default;

            var externalId = feature.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var name = companyMetaData.ValueKind != JsonValueKind.Undefined &&
                       companyMetaData.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString()
                : null;
            var addressText = companyMetaData.ValueKind != JsonValueKind.Undefined &&
                              companyMetaData.TryGetProperty("address", out var addressElement)
                ? addressElement.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(name))
                continue;

            items.Add(new YandexOrganizationSearchResult
            {
                ExternalId = externalId,
                Name = name,
                AddressText = addressText,
                Latitude = poiLatitude,
                Longitude = poiLongitude,
                DistanceMeters = CalculateDistanceMeters(latitude, longitude, poiLatitude, poiLongitude)
            });
        }

        return items
            .OrderBy(x => x.DistanceMeters)
            .ToList();
    }

    private static string BuildSpn(int radiusMeters)
    {
        var degrees = Math.Max(radiusMeters / 111_000d, 0.001d);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{degrees:F6},{degrees:F6}");
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

        var a = Math.Pow(Math.Sin(dLat / 2), 2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return decimal.Round((decimal)(earthRadiusMeters * c), 2);
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;
}
