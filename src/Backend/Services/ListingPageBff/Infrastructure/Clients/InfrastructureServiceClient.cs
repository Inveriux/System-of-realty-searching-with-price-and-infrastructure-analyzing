using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using ListingPageBff.Infrastructure.Clients.Abstractions;
using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients;

public sealed class InfrastructureServiceClient(HttpClient httpClient) : IInfrastructureServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<DownstreamInfrastructureSummaryDto?> GetSummaryAsync(Guid locationId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/infrastructure/{locationId}/summary", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var wrapped = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamInfrastructureSummaryDto>>(stream, JsonOptions, cancellationToken);
        return wrapped?.IsSuccess == true ? wrapped.Data : null;
    }

    public async Task<DownstreamInfrastructureSummaryDto?> RefreshAsync(
        DownstreamRefreshInfrastructureRequestDto request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/infrastructure/refresh", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var wrapped = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamInfrastructureSummaryDto>>(stream, JsonOptions, cancellationToken);
        return wrapped?.IsSuccess == true ? wrapped.Data : null;
    }
}
