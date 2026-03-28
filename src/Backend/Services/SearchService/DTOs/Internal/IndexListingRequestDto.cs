namespace SearchService.DTOs.Internal;

public sealed class IndexListingRequestDto
{
    public Guid ListingId { get; init; }
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
    public decimal AreaTotal { get; init; }
    public string? MainMediaUrl { get; init; }
    public int MediaCount { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public string? ListingDetailsType { get; init; }
}
