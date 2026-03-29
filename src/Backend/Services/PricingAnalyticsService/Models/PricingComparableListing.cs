namespace PricingAnalyticsService.Models;

public sealed class PricingComparableListing
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string? AddressText { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ListingType { get; set; }
    public string? BuildingType { get; set; }
    public string? ListingDetailsType { get; set; }
    public decimal? PriceAmount { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal AreaTotal { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
