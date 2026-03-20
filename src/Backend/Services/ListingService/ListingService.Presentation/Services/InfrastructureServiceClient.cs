using System.Text;
using System.Text.Json;
using ListingService.Application.Contracts.Services;
using Microsoft.Extensions.Options;

namespace ListingService.Presentation.Services;

public sealed class InfrastructureServiceClient(HttpClient httpClient, IOptions<InfrastructureServiceOptions> options)
    : IInfrastructureServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly InfrastructureServiceOptions _ = options.Value;

    public async Task<InfrastructureResolvedLocation?> ResolveLocationAsync(string addressText, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new { addressText });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("internal/locations/resolve", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var responsePayload = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var result = JsonSerializer.Deserialize<InfrastructureClientResult<ResolvedLocationDto>>(responsePayload, JsonOptions);
            return result?.Data is null
                ? null
                : new InfrastructureResolvedLocation(result.Data.Id, result.Data.Latitude, result.Data.Longitude);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<bool> RefreshInfrastructureAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new { locationId });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("internal/infrastructure/refresh-for-location", content, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
