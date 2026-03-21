using ListingService.Domain.Abstractions;
using ListingService.Domain.Enums;

namespace ListingService.Domain.Models;

public class Listing : AuditableEntity<Guid>
{
    public static Listing Create() => new() {Id = Guid.NewGuid()};

    public Guid AgentId { get; set; }
    public string AddressText { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal? PriceAmount { get; set; } 
    public string? PriceCurrency { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? PricePeriod { get; set; }
    public decimal AreaTotal { get; set; }
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }


    public Guid AddressId { get; set; }
    public Guid? BuildingTypeId { get; set; }
    public BuildingType? BuildingType { get; set; }
    public Guid ListingTypeId { get; set; }
    public ListingType ListingType { get; set; } = null!;
    public ICollection<ListingMedia>? Media { get; set; } 
    public ICollection<ListingPriceHistory>? PriceHistory { get; set; }

    //TODO is need standart value
    public string Status {get; set;} = ListingStatus.Active;

    public string ListingDetailsType {get; set;} = null!;
    public IListingDetails? ListingDetails { get; set; }


    public void ChangePrice(ListingPriceHistory? priceHistory)
    {
        if(priceHistory is null)
            return;

        PriceAmount = priceHistory.PriceAmount;
        PriceCurrency = priceHistory.PriceCurrency;
        PriceAmountByn = priceHistory.PriceAmountByn;
        PriceAmountUsd = priceHistory.PriceAmountUsd;
    }
}
