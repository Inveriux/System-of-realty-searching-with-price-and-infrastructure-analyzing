using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Application.Common.Lookups.Converters;
using ListingService.Domain.Abstractions;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class CreateListingDto
{ 
    public Guid AgentId { get; set; }
    public required string AddressText { get; init; } 
    public required string Title { get; init; }
    public string? Description { get; set; }
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public string? PricePeriod { get; set; }
    public decimal AreaTotal { get; set; }
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public Guid AddressId { get; set; }
    public string? BuildingType { get; set; }
    public required string ListingType { get; set; }
    public string? Status { get; set; }
    public required string ListingDetailsType { get; init; }

    public required IListingDetailsDto ListingDetails { get; init; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateListingDto, Listing>()
                .ConstructUsing(_ => Listing.Create())
                .AfterMap((_, dest) =>
                {
                    dest.BuildingTypeId = dest.BuildingType?.Id;
                    dest.BuildingType = null; 
                    dest.ListingTypeId = dest.ListingType?.Id
                        ?? throw new AutoMapperMappingException("ListingType is required.");
                    dest.ListingType = null;
                });
            CreateMap<ApartmentDetailsDto, ApartmentDetails>()
                .AfterMap((_, dest) =>
                {
                    dest.BathroomTypeId = dest.BathroomType?.Id;
                    dest.BathroomType = null;
                    dest.RenovationTypeId = dest.RenovationType?.Id;
                    dest.RenovationType = null;
                    dest.BuildingMaterialId = dest.BuildingMaterial?.Id;
                    dest.BuildingMaterial = null;
                });
            CreateMap<HouseDetailsDto, HouseDetails>()
                .AfterMap((_, dest) =>
                {
                    dest.WallMaterialId = dest.WallMaterial?.Id
                        ?? throw new AutoMapperMappingException("WallMaterial is required.");
                    dest.HeatingTypeId = dest.HeatingType?.Id
                        ?? throw new AutoMapperMappingException("HeatingType is required.");
                    dest.SewerTypeId = dest.SewerType?.Id
                        ?? throw new AutoMapperMappingException("SewerType is required.");
                    dest.GasTypeId = dest.GasType?.Id
                        ?? throw new AutoMapperMappingException("GasType is required.");
                    dest.RoofMaterialId = dest.RoofMaterial?.Id
                        ?? throw new AutoMapperMappingException("RoofMaterial is required.");
                    dest.WaterSupplyTypeId = dest.WaterSupplyType?.Id
                        ?? throw new AutoMapperMappingException("WaterSupplyType is required.");
                });
            CreateMap<WarehouseDetailsDto, WarehouseDetails>()
                .AfterMap((_, dest) =>
                {
                    dest.ClassTypeId = dest.ClassType?.Id
                        ?? throw new AutoMapperMappingException("ClassType is required.");
                    dest.HeatingTypeId = dest.HeatingType?.Id
                        ?? throw new AutoMapperMappingException("HeatingType is required.");
                });
            CreateMap<OfficeDetailsDto, OfficeDetails>()
                .AfterMap((_, dest) =>
                {
                    dest.OfficeClassId = dest.OfficeClass?.Id
                        ?? throw new AutoMapperMappingException("OfficeClass is required.");
                    dest.LayoutTypeId = dest.LayoutType?.Id
                        ?? throw new AutoMapperMappingException("LayoutType is required.");
                    dest.RenovationTypeId = dest.RenovationType?.Id
                        ?? throw new AutoMapperMappingException("RenovationType is required.");
                    dest.ParkingTypeId = dest.ParkingType?.Id
                        ?? throw new AutoMapperMappingException("ParkingType is required.");
                });
            CreateMap<LandDetailsDto, LandDetails>()
                .AfterMap((_, dest) =>
                {
                    dest.LandCategoryId = dest.LandCategory?.Id
                        ?? throw new AutoMapperMappingException("LandCategory is required.");
                    dest.RoadAccessTypeId = dest.RoadAccessType?.Id
                        ?? throw new AutoMapperMappingException("RoadAccessType is required.");
                });

            CreateMap<IListingDetailsDto, IListingDetails>()
                .Include<ApartmentDetailsDto, ApartmentDetails>()
                .Include<HouseDetailsDto, HouseDetails>()
                .Include<LandDetailsDto, LandDetails>()
                .Include<WarehouseDetailsDto, WarehouseDetails>()
                .Include<OfficeDetailsDto, OfficeDetails>();
            
            CreateMap<string?, BuildingType?>()
                .ConvertUsing<LookupByNameOrNullConverter<BuildingType>>();

            CreateMap<string, ListingType>()
                .ConvertUsing<LookupByNameConverter<ListingType>>();
        }
    }
}
