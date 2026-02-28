using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Application.Common.Lookups.Converters;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class LandDetailsDto : IListingDetailsDto
{
    public decimal? PlotArea { get; set; }
    public string? AllowedUse { get; set; }
    public bool? HasElectricity { get; set; }
    public bool? HasGas { get; set; }
    public bool? HasWater { get; set; }
    public bool? HasSewer { get; set; }
    public bool? HasBuildings { get; set; }
    public string? Listing { get; set; }
    public string? LandCategory { get; set; }
    public string? RoadAccessType { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<LandDetails, LandDetailsDto>();
            CreateMap<string?, LandCategory?>()
                .ConvertUsing<LookupByNameOrNullConverter<LandCategory>>();
            CreateMap<string?, RoadAccessType?>()
                .ConvertUsing<LookupByNameOrNullConverter<RoadAccessType>>();
        }
    }
}
