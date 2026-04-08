using System.Net;
using System.Text.Json;
using ChatService.Clients.Listings.Abstractions;

namespace ChatService.Clients.Listings;

public sealed class ListingCatalogClient(HttpClient httpClient) : IListingCatalogClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ListingSummarySnapshot?> GetListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/listings/details/{listingId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var envelope = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamListingDto>>(stream, JsonOptions, cancellationToken);
        if (envelope?.IsSuccess != true || envelope.Data is null)
        {
            return null;
        }

        return new ListingSummarySnapshot(
            envelope.Data.Id,
            envelope.Data.AgentId,
            envelope.Data.Title,
            envelope.Data.PriceAmountByn ?? envelope.Data.PriceAmountUsd ?? envelope.Data.PriceAmount,
            envelope.Data.PriceCurrency ?? (envelope.Data.PriceAmountByn is not null ? "BYN" : envelope.Data.PriceAmountUsd is not null ? "USD" : null),
            envelope.Data.MainMedia ?? envelope.Data.Media.FirstOrDefault()?.Url);
    }
}

public sealed record ListingSummarySnapshot(
    Guid Id,
    Guid AgentId,
    string Title,
    decimal? PriceAmount,
    string? PriceCurrency,
    string? MainImageUrl);

internal sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
}

internal sealed class DownstreamListingDto
{
    public Guid Id { get; init; }
    public Guid AgentId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal? PriceAmount { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
    public string? PriceCurrency { get; init; }
    public string? MainMedia { get; init; }
    public IReadOnlyCollection<DownstreamListingMediaDto> Media { get; init; } = [];
}

internal sealed class DownstreamListingMediaDto
{
    public string Url { get; init; } = string.Empty;
}
