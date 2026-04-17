using System.Text.Json;
using Microsoft.Extensions.Options;
using SearchService.DTOs.Internal;
using SearchService.Services.InternalClients.Abstractions;

namespace SearchService.Services.InternalClients;

public sealed class ListingServiceClient(HttpClient httpClient, IOptions<ListingServiceClientOptions> options) : IListingServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient = httpClient;
    private readonly ListingServiceClientOptions _ = options.Value;

    public async Task<IndexListingRequestDto?> GetListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"api/listings/{listingId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var wrapped = JsonSerializer.Deserialize<ApiResult<ListingPayload>>(payload, JsonOptions);
            if (wrapped?.IsSuccess != true || wrapped.Data is null)
                return null;

            return new IndexListingRequestDto
            {
                ListingId = wrapped.Data.Id,
                AgentId = wrapped.Data.AgentId,
                Title = wrapped.Data.Title,
                Description = wrapped.Data.Description,
                AddressText = wrapped.Data.AddressText,
                LocationId = wrapped.Data.LocationId,
                Latitude = wrapped.Data.Latitude,
                Longitude = wrapped.Data.Longitude,
                ListingType = wrapped.Data.ListingType,
                BuildingType = wrapped.Data.BuildingType,
                PriceAmount = wrapped.Data.PriceAmount,
                PriceAmountByn = wrapped.Data.PriceAmountByn,
                PriceAmountUsd = wrapped.Data.PriceAmountUsd,
                PriceCurrency = wrapped.Data.PriceCurrency,
                PricePeriod = wrapped.Data.PricePeriod,
                AreaTotal = wrapped.Data.AreaTotal,
                MainMediaUrl = wrapped.Data.MainMedia,
                MediaCount = wrapped.Data.MediaCount,
                PublishedDate = wrapped.Data.PublishedDate,
                ListingDetailsType = wrapped.Data.ListingDetailsType
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class ApiResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
    }

    private sealed class ListingPayload
    {
        public Guid Id { get; init; }
        public Guid AgentId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? AddressText { get; init; }
        public Guid? LocationId { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public string? ListingType { get; init; }
        public string? BuildingType { get; init; }
        public decimal? PriceAmount { get; init; }
        public decimal? PriceAmountByn { get; init; }
        public decimal? PriceAmountUsd { get; init; }
        public string? PriceCurrency { get; init; }
        public string? PricePeriod { get; init; }
        public decimal AreaTotal { get; init; }
        public string? MainMedia { get; init; }
        public int MediaCount { get; init; }
        public DateTimeOffset? PublishedDate { get; init; }
        public string? ListingDetailsType { get; init; }
    }
}
