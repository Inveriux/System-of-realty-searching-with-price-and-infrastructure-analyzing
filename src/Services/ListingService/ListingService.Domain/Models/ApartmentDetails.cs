namespace ListingService.Domain.Models;

public class ApartmentDetails
{
    public int? Rooms { get; set; }
    public int? Bedrooms { get; set; }
    public int? FloorsTotal { get; set; }
    public int? Levels { get; set; }
    public int? Floor { get; set; }
    public decimal? AreaLiving { get; set; }
    public decimal? AreaKitchen { get; set; }
    public bool? HasBalcony { get; set; }
    public bool? HasLoggia { get; set; }
    public bool? HasElevator { get; set; }
    public decimal? CeilingHeight { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
    public Guid BathroomTypeId { get; set; }
    public BathroomType? BathroomType { get; set; }
    public Guid RenovationTypeId { get; set; }
    public RenovationType? RenovationType { get; set; }
    public Guid BuildingMaterialId { get; set; }
    public BuildingMaterial? BuildingMaterial { get; set; }
}
