using System.Net;
using System.Text.Json;
using ListingPageBff.Infrastructure.Clients.Abstractions;
using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients;

public sealed class ListingServiceClient(HttpClient httpClient) : IListingServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<DownstreamDetailedListingDto?> GetDetailedListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/listings/details/{listingId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var wrapped = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamDetailedListingDto>>(stream, JsonOptions, cancellationToken);
        return wrapped?.IsSuccess == true ? wrapped.Data : null;
    }
}
