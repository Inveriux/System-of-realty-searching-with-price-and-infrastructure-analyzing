namespace ListingService.Application.DTOs.Listings;

public record ListingPriceHistoryDto
{
    public Guid Id { get; set; }
    public DateTimeOffset ChangedAt { get; set; } 
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? Comment { get; set; }
}
