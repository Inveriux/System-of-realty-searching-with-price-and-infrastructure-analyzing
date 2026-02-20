using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Lookups;


public class LookupTypeNameDto
{
    public required string TypeName { get; set; } 

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BathroomType, LookupTypeNameDto>();
            CreateMap<BuildingMaterial, LookupTypeNameDto>();
            CreateMap<BuildingType, LookupTypeNameDto>();
            CreateMap<ClassType, LookupTypeNameDto>();
            CreateMap<GasType, LookupTypeNameDto>();
            CreateMap<HeatingType, LookupTypeNameDto>();
            CreateMap<LandCategory, LookupTypeNameDto>();
            CreateMap<ListingType, LookupTypeNameDto>();
            CreateMap<RenovationType, LookupTypeNameDto>();
            CreateMap<RoadAccessType, LookupTypeNameDto>();
            CreateMap<SewerType, LookupTypeNameDto>();
            CreateMap<WallMaterial, LookupTypeNameDto>();
            CreateMap<WaterSupplyType, LookupTypeNameDto>();
        }
    }
}
