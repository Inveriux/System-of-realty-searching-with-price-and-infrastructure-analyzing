using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Application.Common.Lookups.Converters;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class OfficeDetailsDto : IListingDetailsDto
{
    public decimal? PlotArea { get; set; }
    public int? Floor { get; set; }
    public int? FloorsTotal { get; set; }
    public int? Levels { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool? Kitchenette { get; set; }
    public string? Listing { get; set; }
    public string? OfficeClass { get; set; }
    public string? LayoutType { get; set; }
    public string? RenovationType { get; set; }
    public string? ParkingType { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<OfficeDetails, OfficeDetailsDto>();
            CreateMap<string?, OfficeClass?>()
                .ConvertUsing<LookupByNameOrNullConverter<OfficeClass>>();
            CreateMap<string?, LayoutType?>()
                .ConvertUsing<LookupByNameOrNullConverter<LayoutType>>();
        }
    }
}
