using System.Text.Json;
using Microsoft.Extensions.Options;
using SearchService.Services.InternalClients.Abstractions;
using SearchService.Services.InternalClients.Contracts;

namespace SearchService.Services.InternalClients;

public sealed class InfrastructureServiceClient(HttpClient httpClient, IOptions<InfrastructureServiceClientOptions> options) : IInfrastructureServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient = httpClient;
    private readonly InfrastructureServiceClientOptions _ = options.Value;

    public async Task<Dictionary<string, decimal>> GetSearchFeaturesAsync(Guid locationId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"internal/infrastructure/{locationId}/search-features", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return [];

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var wrapped = JsonSerializer.Deserialize<InternalResult<Dictionary<string, decimal>>>(payload, JsonOptions);
            return wrapped?.Data ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task<InfrastructureScorePayloadDto?> GetInfrastructureScoreAsync(Guid locationId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"internal/infrastructure/{locationId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var wrapped = JsonSerializer.Deserialize<InternalResult<InfrastructureScorePayloadDto>>(payload, JsonOptions);
            return wrapped?.Data;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class InternalResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
    }
}
