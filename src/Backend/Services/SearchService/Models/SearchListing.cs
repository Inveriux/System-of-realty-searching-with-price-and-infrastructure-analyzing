namespace SearchService.Models;

public sealed class SearchListing
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid AgentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AddressText { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ListingType { get; set; }
    public string? BuildingType { get; set; }
    public string? ListingDetailsType { get; set; }
    public decimal? PriceAmount { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal AreaTotal { get; set; }
    public string? MainMediaUrl { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public decimal? InfrastructureScore { get; set; }
    public decimal? EducationScore { get; set; }
    public decimal? HealthcareScore { get; set; }
    public decimal? TransportScore { get; set; }
    public decimal? CommerceScore { get; set; }
    public decimal? LeisureScore { get; set; }
    public int? EducationCount { get; set; }
    public int? HealthcareCount { get; set; }
    public int? TransportCount { get; set; }
    public int? CommerceCount { get; set; }
    public int? LeisureCount { get; set; }
    public decimal? EducationNearestMeters { get; set; }
    public decimal? HealthcareNearestMeters { get; set; }
    public decimal? TransportNearestMeters { get; set; }
    public decimal? CommerceNearestMeters { get; set; }
    public decimal? LeisureNearestMeters { get; set; }
    public string SearchFeaturesJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
