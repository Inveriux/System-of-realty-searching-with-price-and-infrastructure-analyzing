using InfrastructureService.Services.Places.Abstractions;

namespace InfrastructureService.Services.Places.Overpass.Abstractions;

public sealed record OverpassMappedPoi(
    string CategoryCode,
    string SubcategoryCode,
    string OsmType,
    ExternalPoiResult Poi);
