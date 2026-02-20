namespace ListingService.Domain.Models;

public class ListingPriceHistory
{
    public Guid Id { get; set; }
    public required DateTime Date { get; set; }
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = string.Empty;
    public decimal PreviousPrice { get; set; }
    public int? SortOrder { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
