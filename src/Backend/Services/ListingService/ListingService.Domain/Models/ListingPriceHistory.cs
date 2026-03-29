namespace ListingService.Domain.Models;

public class ListingPriceHistory
{
    public Guid Id { get; set; }
    public DateTimeOffset ChangedAt { get; set; } 
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? Comment { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }


    public bool IsPriceEqual(ListingPriceHistory? priceHistory)
    {
        if(priceHistory is null)
            return false;

        return priceHistory.PriceAmount == PriceAmount 
            && priceHistory.PriceCurrency == PriceCurrency
            && priceHistory.PriceAmountByn == PriceAmountByn
            && priceHistory.PriceAmountUsd == PriceAmountUsd;
    }
}
