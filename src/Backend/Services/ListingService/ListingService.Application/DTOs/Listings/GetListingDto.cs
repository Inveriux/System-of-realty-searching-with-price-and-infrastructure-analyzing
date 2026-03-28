using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class GetListingDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string? AddressText { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public required string Title { get; init; }
    public string? Description { get; set; }
    public decimal? PriceAmount { get; set; } 
    public string? PriceCurrency { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? PricePeriod { get; set; }
    public required decimal AreaTotal { get; init; }//TODO implement
    public int? BuildYear { get; set; }
    public string? MainMedia { get; set; }
    public int MediaCount { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public string? BuildingType { get; set; }
    public required string ListingType { get; init; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public string? ListingDetailsType { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Listing, GetListingDto>()
                .ForMember(x => x.MainMedia, y => y.MapFrom(s =>
                    s.Media == null
                        ? null
                        : s.Media.Where(m => m.IsMainMedia)
                                .Select(m => m.Url)
                                .FirstOrDefault()))
                .ForMember(x => x.MediaCount, y => y.MapFrom(s => s.Media == null ? 0 : s.Media.Count));
        }
    }
}
