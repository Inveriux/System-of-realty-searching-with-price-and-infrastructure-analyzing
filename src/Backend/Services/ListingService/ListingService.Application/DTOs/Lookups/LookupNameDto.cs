using ListingService.Application.DTOs.Lookups.Abstractions;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Lookups;


public class LookupNameDto : ILookupDto
{
    public required string Name { get; init; } 

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BathroomType, LookupNameDto>().ReverseMap();
            CreateMap<BuildingMaterial, LookupNameDto>().ReverseMap();
            CreateMap<BuildingType, LookupNameDto>().ReverseMap();
            CreateMap<ClassType, LookupNameDto>().ReverseMap();
            CreateMap<GasType, LookupNameDto>().ReverseMap();
            CreateMap<HeatingType, LookupNameDto>().ReverseMap();
            CreateMap<LandCategory, LookupNameDto>().ReverseMap();
            CreateMap<LayoutType, LookupNameDto>().ReverseMap();
            CreateMap<ListingType, LookupNameDto>().ReverseMap();
            CreateMap<OfficeClass, LookupNameDto>().ReverseMap();
            CreateMap<RenovationType, LookupNameDto>().ReverseMap();
            CreateMap<RoadAccessType, LookupNameDto>().ReverseMap();
            CreateMap<SewerType, LookupNameDto>().ReverseMap();
            CreateMap<WallMaterial, LookupNameDto>().ReverseMap();
            CreateMap<WaterSupplyType, LookupNameDto>().ReverseMap();
        }
    }
}
