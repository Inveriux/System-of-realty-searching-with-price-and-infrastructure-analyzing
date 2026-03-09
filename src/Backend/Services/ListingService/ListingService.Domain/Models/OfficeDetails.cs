using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;

public class OfficeDetails : IListingDetails
{
    public decimal? PlotArea { get; set; }
    public int? Floor { get; set; }
    public int? FloorsTotal { get; set; }
    public int? Levels { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool? Kitchenette { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
    public Guid? OfficeClassId { get; set; }
    public OfficeClass? OfficeClass { get; set; }
    public Guid? LayoutTypeId { get; set; }
    public LayoutType? LayoutType { get; set; }
    public Guid? RenovationTypeId { get; set; }
    public LayoutType? RenovationType { get; set; }
    public Guid? ParkingTypeId { get; set; }
    public LayoutType? ParkingType { get; set; }
}
