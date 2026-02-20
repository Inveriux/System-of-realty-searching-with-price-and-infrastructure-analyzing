using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Lookups;

public class LookupClassNameDto
{
    public required string ClassName { get; set; } 

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<LayoutType, LookupClassNameDto>();
            CreateMap<OfficeClass, LookupClassNameDto>();
        }
    }
}
