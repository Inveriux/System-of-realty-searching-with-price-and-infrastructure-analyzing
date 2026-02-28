using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Domain.Abstractions;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class GetDetailedListingDto
{
    public Guid Id { get; set; }
    // public Guid AgentId { get; set; }
    public string? AddressText { get; set; }
    public required string Title { get; init; }
    public string? Description { get; set; }
    //TODO check price properties
    public decimal? PriceAmount { get; set; } 
    public string? PriceCurrency { get; set; }
    public string? PricePeriod { get; set; }
    public required decimal AreaTotal { get; init; }//TODO implement
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public string? BuildingType { get; set; }
    public required string ListingType { get; init; }
    public ICollection<ListingMedia>? Media { get; set; } 
    public ICollection<ListingPriceHistory>? PriceHistory { get; set; }
    public required IListingDetailsDto ListingDetails { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Listing, GetDetailedListingDto>();

            CreateMap<IListingDetails, IListingDetailsDto>()
                .Include<ApartmentDetails, ApartmentDetailsDto>()
                .Include<HouseDetails, HouseDetailsDto>()
                .Include<LandDetails, LandDetailsDto>()
                .Include<WarehouseDetails, WarehouseDetailsDto>()
                .Include<OfficeDetails, OfficeDetailsDto>();    

            CreateMap<ILookupEntity, string>()
                .ConvertUsing(src => src.Name);
            
            CreateMap<ILookupEntity?, string?>()
                .ConvertUsing(src => src == null ? null : src.Name);
        }
    }
}