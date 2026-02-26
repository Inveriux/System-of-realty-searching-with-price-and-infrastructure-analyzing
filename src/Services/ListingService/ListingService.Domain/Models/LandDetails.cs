using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;

public class LandDetails : IListingDetails
{
    public decimal? PlotArea { get; set; }
    public string? AllowedUse { get; set; }
    public bool? HasElectricity { get; set; }
    public bool? HasGas { get; set; }
    public bool? HasWater { get; set; }
    public bool? HasSewer { get; set; }
    public bool? HasBuildings { get; set; }

    public string ListingDetailsType { get; set; } = null!;
    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
    public Guid LandCategoryId { get; set; }
    public LandCategory? LandCategory { get; set; }
    public Guid RoadAccessTypeId { get; set; }
    public RoadAccessType? RoadAccessType { get; set; }
}
