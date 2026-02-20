using ListingService.Application.DTOs.Listing.Abstractions;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listing;

public class ApartmentDetailDto : IDetailListingDto
{
    public int? Rooms { get; set; }
    public int? Bedrooms { get; set; }
    public int? FloorsTotal { get; set; }
    public int? Levels { get; set; }
    public int? Floor { get; set; }
    public decimal? AreaLiving { get; set; }
    public decimal? AreaKitchen { get; set; }
    public bool? HasBalcony { get; set; }
    public bool? HasLoggia { get; set; }
    public bool? HasElevator { get; set; }
    public decimal? CeilingHeight { get; set; }
    public string? Listing { get; set; }
    public string? BathroomType { get; set; }
    public string? RenovationType { get; set; }
    public string? BuildingMaterial { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApartmentDetails, ApartmentDetailDto>();
        }
    }
}