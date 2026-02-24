using ListingService.Domain.Abstractions;
using ListingService.Domain.Enums;

namespace ListingService.Domain.Models;

public class Listing : Entity<Guid>
{
    public Guid AgentId { get; set; }
    public string AddressText { get; set; } = string.Empty;
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    //TODO check price properties
    public decimal? PriceAmount { get; set; } 
    public string? PriceCurrency { get; set; }
    public string? PricePeriod { get; set; }
    public required decimal AreaTotal { get; set; }
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }


    public Guid AddressId { get; set; }
    public Guid BuildingTypeId { get; set; }
    public BuildingType? BuildingType { get; set; }
    public Guid ListingTypeId { get; set; }
    public ListingType? ListingType { get; set; }
    public ICollection<ListingMedia>? Media { get; set; } 
    public ICollection<ListingPriceHistory>? PriceHistory { get; set; }

    //TODO is need standart value
    public string Status {get; set;} = ListingStatus.Active;
}
