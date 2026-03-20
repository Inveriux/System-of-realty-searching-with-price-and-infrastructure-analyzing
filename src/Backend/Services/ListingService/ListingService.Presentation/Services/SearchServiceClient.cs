using System.Text;
using System.Text.Json;
using ListingService.Application.Contracts.Services;
using Microsoft.Extensions.Options;

namespace ListingService.Presentation.Services;

public sealed class SearchServiceClient(HttpClient httpClient, IOptions<SearchServiceOptions> options)
    : ISearchServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly SearchServiceOptions _ = options.Value;

    public async Task<bool> IndexListingAsync(SearchListingIndexRequest request, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("internal/search/index-listing", content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync($"internal/search/listings/{listingId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
