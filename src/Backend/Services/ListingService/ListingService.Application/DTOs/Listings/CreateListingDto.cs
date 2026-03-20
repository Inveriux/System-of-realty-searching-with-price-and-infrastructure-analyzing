using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Domain.Abstractions;
using ListingService.Domain.Models;
using AutoMapper;
using System.Text.Json.Serialization;

namespace ListingService.Application.DTOs.Listings;

public class CreateListingDto
{ 
    public Guid AgentId { get; set; }
    public required string AddressText { get; init; } 
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
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
    [JsonIgnore]
    public List<string>? MediaUrls { get; set; }

    public required IListingDetailsDto ListingDetails { get; init; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateListingDto, Listing>()
                .ConstructUsing(_ => Listing.Create())
                .ForMember(x => x.BuildingType, y => y.Ignore())
                .ForMember(x => x.ListingType, y => y.Ignore())
                .ForMember(x => x.Media, opt => opt.MapFrom(s => s.MediaUrls))
                .AfterMap((src, dest) =>
                {
                    if (dest.Media is null || dest.Media.Count == 0)
                        return;

                    var mainMediaUrl = src.MediaUrls?
                        .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));

                    if (string.IsNullOrWhiteSpace(mainMediaUrl))
                        return;

                    var mainMedia = dest.Media.FirstOrDefault(m => m.Url == mainMediaUrl);
                    if (mainMedia is not null)
                        mainMedia.IsMainMedia = true;
                });
                    

            CreateMap<string, ListingMedia>()
                .ConstructUsing(url => new ListingMedia
                {
                    Id = Guid.NewGuid(),
                    Url = url
                });

            CreateMap<ApartmentDetailsDto, ApartmentDetails>()
                .ForMember(x => x.Listing, y => y.Ignore())
                .ForMember(x => x.BathroomType, y => y.Ignore())
                .ForMember(x => x.RenovationType, y => y.Ignore())
                .ForMember(x => x.BuildingMaterial, y => y.Ignore());

            CreateMap<HouseDetailsDto, HouseDetails>()
                .ForMember(x => x.Listing, y => y.Ignore())
                .ForMember(x => x.WallMaterial, y => y.Ignore())
                .ForMember(x => x.HeatingType, y => y.Ignore())
                .ForMember(x => x.SewerType, y => y.Ignore())
                .ForMember(x => x.GasType, y => y.Ignore())
                .ForMember(x => x.RoofMaterial, y => y.Ignore())
                .ForMember(x => x.WaterSupplyType, y => y.Ignore());

            CreateMap<WarehouseDetailsDto, WarehouseDetails>()
                .ForMember(x => x.Listing, y => y.Ignore())
                .ForMember(x => x.ClassType, y => y.Ignore())
                .ForMember(x => x.HeatingType, y => y.Ignore());

            CreateMap<OfficeDetailsDto, OfficeDetails>()
                .ForMember(x => x.Listing, y => y.Ignore())
                .ForMember(x => x.OfficeClass, y => y.Ignore())
                .ForMember(x => x.LayoutType, y => y.Ignore())
                .ForMember(x => x.RenovationType, y => y.Ignore())
                .ForMember(x => x.ParkingType, y => y.Ignore());

            CreateMap<LandDetailsDto, LandDetails>()
                .ForMember(x => x.Listing, y => y.Ignore())
                .ForMember(x => x.LandCategory, y => y.Ignore())
                .ForMember(x => x.RoadAccessType, y => y.Ignore());
            
            CreateMap<IListingDetailsDto, IListingDetails>()
                .Include<ApartmentDetailsDto, ApartmentDetails>()
                .Include<HouseDetailsDto, HouseDetails>()
                .Include<LandDetailsDto, LandDetails>()
                .Include<WarehouseDetailsDto, WarehouseDetails>()
                .Include<OfficeDetailsDto, OfficeDetails>();
        }
    }
}
