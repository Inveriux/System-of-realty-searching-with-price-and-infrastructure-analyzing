namespace SearchService.Models;

public sealed class SearchListing
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AddressText { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ListingType { get; set; }
    public string? BuildingType { get; set; }
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal AreaTotal { get; set; }
    public string? MainMediaUrl { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public decimal? InfrastructureScore { get; set; }
    public string SearchFeaturesJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
