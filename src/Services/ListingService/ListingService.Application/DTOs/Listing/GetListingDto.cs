using ListingService.Application.DTOs.Listing.Abstractions;
using ListingService.Domain.Enums;
using AutoMapper;

using ListingModel = ListingService.Domain.Models.Listing; //TODO BRUH

namespace ListingService.Application.DTOs.Listing;

public class GetListingDto
{
    // public Guid AgentId { get; set; }
    public string AddressText { get; set; } = string.Empty;
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    //TODO check price properties
    public decimal? PriceAmount { get; set; } 
    public string? PriceCurrency { get; set; }
    public ListingPricePeriod? PricePeriod { get; set; }
    public required decimal AreaTotal { get; set; }//TODO implement
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public string? BuildingType { get; set; }
    public required string ListingType { get; set; }
    // public ICollection<ListingMedia>? Media { get; set; } 
    // public ICollection<ListingPriceHistory>? PriceHistory { get; set; }
    public IDetailListingDto? Detail { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ListingModel, GetListingDto>();
        }
    }
}