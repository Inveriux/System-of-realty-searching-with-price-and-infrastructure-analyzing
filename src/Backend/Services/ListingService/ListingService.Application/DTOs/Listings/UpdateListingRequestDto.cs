using ListingService.Application.DTOs.Listings.Abstractions;

namespace ListingService.Application.DTOs.Listings;

public sealed class UpdateListingRequestDto
{
    public Guid AgentId { get; set; }
    public required string AddressText { get; init; }
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public required string Title { get; init; }
    public string? Description { get; set; }
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? PricePeriod { get; set; }
    public decimal AreaTotal { get; set; }
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public Guid AddressId { get; set; }
    public string? BuildingType { get; set; }
    public required string ListingType { get; set; }
    public string? Status { get; set; }
    public required string ListingDetailsType { get; init; }
    public List<string>? MediaUrls { get; set; }
    public required IListingDetailsDto ListingDetails { get; init; }
}
