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
        var endpoints = GetEndpoints();
        var attempts = GetAttemptCount();

        var endpointList = endpoints.ToArray();
        for (var endpointIndex = 0; endpointIndex < endpointList.Length; endpointIndex++)
        {
            var endpoint = endpointList[endpointIndex];
            var hasMoreEndpoints = endpointIndex < endpointList.Length - 1;

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri(endpoint))
                {
                    Content = new StringContent($"data={Uri.EscapeDataString(query)}", Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                try
                {
                    using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                        var payload = await JsonSerializer.DeserializeAsync<OverpassResponseDto>(stream, JsonOptions, cancellationToken);
                        if (ShouldFallbackForPayload(payload))
                        {
                            logger.LogWarning(
                                "Overpass API returned a degraded success payload. Endpoint: {Endpoint}. Attempt: {Attempt}/{Attempts}. Remark: {Remark}",
                                endpoint,
                                attempt,
                                attempts,
                                payload?.Remark);

                            if (attempt == attempts)
                                break;

                            continue;
                        }

                        var elements = payload?.Elements?
                            .Where(static x => x is not null)
                            .Select(static x => new OverpassElement
                            {
                                Id = x.Id,
                                Type = x.Type,
                                Latitude = x.Lat ?? x.Center?.Lat,
                                Longitude = x.Lon ?? x.Center?.Lon,
                                Tags = x.Tags
                            })
                            .Where(static x => x.Latitude.HasValue && x.Longitude.HasValue && x.Tags.Count > 0)
                            .ToArray()
                            ?? [];

                        if (_options.UsePublicInstance && elements.Length == 0 && hasMoreEndpoints)
                        {
                            logger.LogWarning(
                                "Overpass API returned an empty success payload. Endpoint: {Endpoint}. Attempt: {Attempt}/{Attempts}. Trying next endpoint.",
                                endpoint,
                                attempt,
                                attempts);

                            break;
                        }

                        return elements;
                    }

                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning(
                        "Overpass API request failed. Endpoint: {Endpoint}. Status: {StatusCode}. Attempt: {Attempt}/{Attempts}. Response: {Response}",
                        endpoint,
                        (int)response.StatusCode,
                        attempt,
                        attempts,
                        Truncate(body, 1500));

                    if (ShouldReturnImmediately(response.StatusCode) || !IsTransient(response.StatusCode) || attempt == attempts)
                        break;
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning(
                        ex,
                        "Overpass API request timed out. Endpoint: {Endpoint}. Attempt: {Attempt}/{Attempts}.",
                        endpoint,
                        attempt,
                        attempts);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogWarning(
                        ex,
                        "Overpass API transient request error. Endpoint: {Endpoint}. Attempt: {Attempt}/{Attempts}.",
                        endpoint,
                        attempt,
                        attempts);
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(
                        ex,
                        "Overpass API response parsing failed. Endpoint: {Endpoint}. Attempt: {Attempt}/{Attempts}.",
                        endpoint,
                        attempt,
                        attempts);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Overpass API unexpected error. Endpoint: {Endpoint}. Attempt: {Attempt}/{Attempts}.",
                        endpoint,
                        attempt,
                        attempts);
                }

                if (attempt < attempts)
                    await Task.Delay(TimeSpan.FromMilliseconds(_options.RetryDelayMs * attempt), cancellationToken);
            }
        }

        logger.LogWarning("All configured Overpass endpoints failed. Returning empty result set.");
        return [];
    }

    private string BuildQuery(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        IReadOnlyCollection<OverpassTagFilter> filters)
    {
        var builder = new StringBuilder();
        builder.Append($"[out:json][timeout:{Math.Max(_options.TimeoutSeconds, 1)}];");
        builder.Append('(');

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

    private IReadOnlyCollection<string> GetEndpoints()
    {
        var endpoints = new List<string>();

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            endpoints.Add(_options.BaseUrl.Trim());

        foreach (var fallback in _options.FallbackBaseUrls)
        {
            if (string.IsNullOrWhiteSpace(fallback))
                continue;

            if (endpoints.Contains(fallback.Trim(), StringComparer.OrdinalIgnoreCase))
                continue;

            endpoints.Add(fallback.Trim());
        }

        return endpoints;
    }

    private static string BuildRequestUri(string endpoint)
    {
        if (endpoint.EndsWith("/interpreter", StringComparison.OrdinalIgnoreCase))
            return endpoint;

        return $"{endpoint.TrimEnd('/')}/interpreter";
    }

    private int GetAttemptCount()
    {
        if (_options.UsePublicInstance)
            return 1;

        return Math.Max(_options.RetryCount, 0) + 1;
    }

    private bool ShouldReturnImmediately(HttpStatusCode statusCode)
        => _options.UsePublicInstance && statusCode == HttpStatusCode.TooManyRequests;

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.RequestTimeout
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout
            or HttpStatusCode.InternalServerError;

    private static bool ShouldFallbackForPayload(OverpassResponseDto? payload)
    {
        if (payload is null || !string.IsNullOrWhiteSpace(payload.Remark) is false)
            return false;

        if (payload.Elements.Count > 0)
            return false;

        return payload.Remark.Contains("runtime error", StringComparison.OrdinalIgnoreCase)
               || payload.Remark.Contains("timed out", StringComparison.OrdinalIgnoreCase)
               || payload.Remark.Contains("timeout", StringComparison.OrdinalIgnoreCase);
    }

    private static string EscapeRegexValue(string value)
        => value.Replace("_", "\\_").Replace(".", "\\.");

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..maxLength];
    }

}
