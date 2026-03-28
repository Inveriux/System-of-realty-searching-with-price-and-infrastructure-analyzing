using System.Net;
using System.Text.Json;
using ListingPageBff.Infrastructure.Clients.Abstractions;
using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients;

public sealed class SearchServiceClient(HttpClient httpClient) : ISearchServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<DownstreamSearchListingDto?> GetListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/search/listings/{listingId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var wrapped = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamSearchListingDto>>(stream, JsonOptions, cancellationToken);
        return wrapped?.IsSuccess == true ? wrapped.Data : null;
    }

    public async Task<bool> DeleteListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.DeleteAsync($"internal/search/listings/{listingId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}
