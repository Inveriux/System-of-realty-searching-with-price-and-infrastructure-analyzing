using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Domain.Models;
using AutoMapper;

namespace ListingService.Application.DTOs.Listings;

public class WarehouseDetailsDto : IListingDetailsDto
{
    public decimal? PlotArea { get; set; }
    public decimal? CeilingHeight { get; set; }
    public decimal? FloorLoadTonsPerSquareMeter { get; set; }
    public int? GateCount { get; set; }
    public bool? HasDockLevelers { get; set; }
    public int? DockCount { get; set; }
    public bool? HasRamps { get; set; }
    public decimal? PowerKw { get; set; }
    public bool? HasOffice { get; set; }
    public decimal? OfficeArea { get; set; }
    public string? ClassType { get; set; }
    public string? HeatingType { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WarehouseDetails, WarehouseDetailsDto>();
        }
    }
}
