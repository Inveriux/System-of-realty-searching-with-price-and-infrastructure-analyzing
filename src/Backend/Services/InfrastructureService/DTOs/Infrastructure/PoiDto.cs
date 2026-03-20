using AutoMapper;
using InfrastructureService.Models;

namespace InfrastructureService.DTOs.Infrastructure;

public sealed class PoiDto
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AddressText { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string Category { get; init; } = string.Empty;
    public decimal DistanceMeters { get; init; }

    private sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<LocationPoiSnapshotItem, PoiDto>()
                .ForMember(x => x.Id, y => y.MapFrom(s => s.PoiId))
                .ForMember(x => x.ExternalId, y => y.MapFrom(s => s.Poi!.ExternalId))
                .ForMember(x => x.Name, y => y.MapFrom(s => s.Poi!.Name))
                .ForMember(x => x.AddressText, y => y.MapFrom(s => s.Poi!.AddressText))
                .ForMember(x => x.Latitude, y => y.MapFrom(s => s.Poi!.Latitude))
                .ForMember(x => x.Longitude, y => y.MapFrom(s => s.Poi!.Longitude))
                .ForMember(x => x.Category, y => y.MapFrom(s => s.Poi!.Category!.Name));
        }
    }
}
