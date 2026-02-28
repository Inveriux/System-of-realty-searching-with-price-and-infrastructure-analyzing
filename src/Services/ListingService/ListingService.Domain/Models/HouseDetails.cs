using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;

public class HouseDetails : IListingDetails
{
    public int? Rooms { get; set; }
    public int? Bedrooms { get; set; }
    public int? Floors { get; set; }
    public decimal? HouseArea { get; set; }
    public decimal? PlotArea { get; set; }
    public bool? HasElectricity { get; set; }
    public bool? HasGarage { get; set; }
    public bool? HasBasement { get; set; }
    public bool? HasFence { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
    public Guid? WallMaterialId { get; set; }
    public WallMaterial? WallMaterial { get; set; }
    public Guid? HeatingTypeId { get; set; }
    public HeatingType? HeatingType { get; set; }
    public Guid? SewerTypeId { get; set; }
    public SewerType? SewerType { get; set; }
    public Guid? GasTypeId { get; set; }
    public GasType? GasType { get; set; }
    public Guid? RoofMaterialId { get; set; }
    public WallMaterial? RoofMaterial { get; set; }
    public Guid? WaterSupplyTypeId { get; set; }
    public WaterSupplyType? WaterSupplyType { get; set; }
}
