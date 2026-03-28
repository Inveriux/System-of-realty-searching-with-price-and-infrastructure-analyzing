using InfrastructureService.Services.Places.Abstractions;

namespace InfrastructureService.Services.Places.Overpass.Abstractions;

public interface IOverpassPoiMapper
{
    OverpassMappedPoi? Map(OverpassElement element, decimal originLatitude, decimal originLongitude);
}
