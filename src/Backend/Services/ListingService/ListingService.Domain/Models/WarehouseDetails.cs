using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;

public class WarehouseDetails : IListingDetails
{
    public decimal? PlotArea { get; set; }
    public decimal? CeilingHeight { get; set; }
    public decimal? FloorLoadTonsPerSquareMeter { get; set; }
    public int? GateCount { get; set; }
    public bool? HasDockLevelers { get; set; }
    public int? DockCount { get; set; }
    public bool? HasRamps { get; set; }
    public decimal? PowerKw { get; set; }
    public bool? HasOffice { get; set; }
    public decimal? OfficeArea { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
    public Guid? ClassTypeId { get; set; }
    public ClassType? ClassType { get; set; }
    public Guid? HeatingTypeId { get; set; }
    public HeatingType? HeatingType { get; set; }
}
