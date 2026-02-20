using AutoMapper;

using BathroomTypeModel = ListingService.Domain.Models.BathroomType; //TODO BRUH

namespace ListingService.Application.DTOs.BathroomType;


public class BathroomTypeDto
{
    public required string TypeName { get; set; } 

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BathroomTypeModel, BathroomTypeDto>();
        }
    }
}

