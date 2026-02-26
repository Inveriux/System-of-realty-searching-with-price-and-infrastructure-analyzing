using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class GetListingDto
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
    // public ICollection<ListingMedia>? Media { get; set; } 
    // public ICollection<ListingPriceHistory>? PriceHistory { get; set; }
    //public IDetailListingDto? Detail { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Listing, GetListingDto>();
        }
    }
}