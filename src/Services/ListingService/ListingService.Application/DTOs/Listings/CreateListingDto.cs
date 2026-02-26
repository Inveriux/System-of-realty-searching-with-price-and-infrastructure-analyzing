using ListingService.Application.DTOs.Listings.Abstractions;
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
    public Guid BuildingTypeId { get; set; }
    public Guid ListingTypeId { get; set; }
    public string? Status { get; set; }
    public required string ListingDetailsType { get; init; }

    public required IListingDetailsDto ListingDetails { get; init; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateListingDto, Listing>()
                .ConstructUsing(_ => Listing.Create());
            CreateMap<ApartmentDetailsDto, ApartmentDetails>();
            CreateMap<HouseDetailsDto, HouseDetails>();
            CreateMap<WarehouseDetailsDto, WarehouseDetails>();
            CreateMap<OfficeDetailsDto, OfficeDetails>();
            CreateMap<LandDetailsDto, LandDetails>();

            CreateMap<IListingDetailsDto, IListingDetails>()
                .Include<ApartmentDetailsDto, ApartmentDetails>()
                .Include<HouseDetailsDto, HouseDetails>()
                .Include<LandDetailsDto, LandDetails>()
                .Include<WarehouseDetailsDto, WarehouseDetails>()
                .Include<OfficeDetailsDto, OfficeDetails>();           
        }
    }
}
