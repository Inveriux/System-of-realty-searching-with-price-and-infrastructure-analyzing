using System.Net;
using System.Text.Json;
using ListingPageBff.Infrastructure.Clients.Abstractions;
using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients;

public sealed class AgentServiceClient(HttpClient httpClient) : IAgentServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<DownstreamSellerPublicProfileDto?> GetPublicProfileAsync(Guid agentId, CancellationToken cancellationToken)
    {
        // Expected downstream endpoint:
        // GET /api/agents/{agentId}/public-profile
        using var response = await httpClient.GetAsync($"api/agents/{agentId}/public-profile", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var wrapped = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamSellerPublicProfileDto>>(stream, JsonOptions, cancellationToken);
        return wrapped?.IsSuccess == true ? wrapped.Data : null;
    }
}
