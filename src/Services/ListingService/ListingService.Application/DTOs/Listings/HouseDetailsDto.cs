using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Application.Common.Lookups.Converters;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class HouseDetailsDto : IListingDetailsDto
{
    public int? Rooms { get; set; }
    public int? Bedrooms { get; set; }
    public int? Floors { get; set; }
    public decimal? HouseArea { get; set; }
    public decimal? PlotArea { get; set; }
    public bool? HasElectricity { get; set; }
    public bool? HasGarage { get; set; }
    public bool? HasBasement { get; set; }
    public bool? HasFence { get; set; }
    public string? Listing { get; set; }
    public string? WallMaterial { get; set; }
    public string? HeatingType { get; set; }
    public string? SewerType { get; set; }
    public string? GasType { get; set; }
    public string? RoofMaterial { get; set; }
    public string? WaterSupplyType { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<HouseDetails, HouseDetailsDto>();
            CreateMap<string?, WallMaterial?>()
                .ConvertUsing<LookupByNameOrNullConverter<WallMaterial>>();
            CreateMap<string?, HeatingType?>()
                .ConvertUsing<LookupByNameOrNullConverter<HeatingType>>();
            CreateMap<string?, SewerType?>()
                .ConvertUsing<LookupByNameOrNullConverter<SewerType>>();
            CreateMap<string?, GasType?>()
                .ConvertUsing<LookupByNameOrNullConverter<GasType>>();
            CreateMap<string?, WaterSupplyType?>()
                .ConvertUsing<LookupByNameOrNullConverter<WaterSupplyType>>();
        }
    }
}
