using System.Net.Http.Json;
using System.Text.Json;
using BasicBuildingBlocks.BusinessResults;
using PricingAnalyticsService.DTOs;

namespace PricingAnalyticsService.Services;

public interface IListingPriceHistoryClient
{
    Task<ListingPriceHistoryDto?> GetPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken);
    Task<ListingPriceHistoryDto?> UpdatePriceAsync(Guid listingId, UpdateListingPriceRequestDto request, CancellationToken cancellationToken);
    Task<ListingPriceHistoryDto?> AddPriceHistoryEntryAsync(Guid listingId, CreateListingPriceHistoryEntryRequestDto request, CancellationToken cancellationToken);
    Task<ListingPriceHistoryDto?> UpdatePriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, UpdateListingPriceHistoryEntryRequestDto request, CancellationToken cancellationToken);
    Task<ListingPriceHistoryDto?> DeletePriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, CancellationToken cancellationToken);
}

public sealed class ListingServiceClientOptions
{
    public const string SectionName = "ListingService";

    public string BaseUrl { get; init; } = string.Empty;
}

public sealed class ListingPriceHistoryClient(HttpClient httpClient) : IListingPriceHistoryClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<ListingPriceHistoryDto?> GetPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Get, $"internal/listings/{listingId}/price-history", body: null, cancellationToken);

    public Task<ListingPriceHistoryDto?> UpdatePriceAsync(Guid listingId, UpdateListingPriceRequestDto request, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Patch, $"internal/listings/{listingId}/price", request, cancellationToken);

    public Task<ListingPriceHistoryDto?> AddPriceHistoryEntryAsync(Guid listingId, CreateListingPriceHistoryEntryRequestDto request, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Post, $"internal/listings/{listingId}/price-history", request, cancellationToken);

    public Task<ListingPriceHistoryDto?> UpdatePriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, UpdateListingPriceHistoryEntryRequestDto request, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Patch, $"internal/listings/{listingId}/price-history/{historyEntryId}", request, cancellationToken);

    public Task<ListingPriceHistoryDto?> DeletePriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Delete, $"internal/listings/{listingId}/price-history/{historyEntryId}", body: null, cancellationToken);

    private async Task<ListingPriceHistoryDto?> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = TryGetProblemMessage(errorText);

            throw new InvalidOperationException(message ?? "Не удалось обновить историю цен объявления.");
        }

        var payload = await response.Content.ReadFromJsonAsync<ResultEnvelope<ListingPriceHistoryDto>>(JsonOptions, cancellationToken);
        return payload?.Data;
    }

    private static string? TryGetProblemMessage(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            var problem = JsonSerializer.Deserialize<ApiProblemDetails>(payload, JsonOptions);
            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return problem.Detail;
            }

            var firstError = problem?.Errors.FirstOrDefault(static error => !string.IsNullOrWhiteSpace(error.Message));
            if (!string.IsNullOrWhiteSpace(firstError?.Message))
            {
                return firstError.Message;
            }
        }
        catch (JsonException)
        {
            // Fall back to the raw payload below.
        }

        return payload.Trim();
    }

    private sealed class ResultEnvelope<T>
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
        public int StatusCode { get; init; }
        public T? Data { get; init; }
    }
}
