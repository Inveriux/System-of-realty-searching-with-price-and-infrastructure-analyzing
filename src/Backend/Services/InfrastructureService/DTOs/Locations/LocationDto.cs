using AutoMapper;
using InfrastructureService.Models;

namespace InfrastructureService.DTOs.Locations;

public sealed class LocationDto
{
    public Guid Id { get; init; }
    public string AddressText { get; init; } = string.Empty;
    public string NormalizedAddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? City { get; init; }
    public string? Street { get; init; }
    public string? HouseNumber { get; init; }

    private sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Location, LocationDto>();
        }
    }
}
