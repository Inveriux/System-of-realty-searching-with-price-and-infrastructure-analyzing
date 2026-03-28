namespace InfrastructureService.Services.Configuration;

public sealed record InfrastructureSubcategoryDefinition(
    string CategoryCode,
    string SubcategoryCode,
    int PrimaryRadiusMeters,
    int? FallbackRadiusMeters);

public sealed record InfrastructureFeatureDefinition(
    string CategoryCode,
    string SubcategoryCode,
    IReadOnlyCollection<string> SourceSubcategories,
    int PrimaryRadiusMeters,
    int? FallbackRadiusMeters);

public static class InfrastructureMetadata
{
    public static readonly IReadOnlyCollection<InfrastructureSubcategoryDefinition> SupportedSubcategories =
    [
        new(InfrastructureCategoryCodes.Commerce, "supermarket", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "hypermarket", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "discount_store", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "convenience_store", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "mall", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "electronics", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "furniture", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "building_store", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "drogerie", 1000, 3000),
        new(InfrastructureCategoryCodes.Commerce, "beauty_service", 1800, 4000),

        new(InfrastructureCategoryCodes.Healthcare, "pharmacy", 1000, 1000),
        new(InfrastructureCategoryCodes.Healthcare, "clinic", 1500, 1500),
        new(InfrastructureCategoryCodes.Healthcare, "hospital", 5000, 5000),
        new(InfrastructureCategoryCodes.Healthcare, "dentistry", 1500, 1500),
        new(InfrastructureCategoryCodes.Healthcare, "laboratory", 1500, 1500),

        new(InfrastructureCategoryCodes.Transport, "bus_stop", 2000, 2000),
        new(InfrastructureCategoryCodes.Transport, "tram_stop", 2000, 2000),
        new(InfrastructureCategoryCodes.Transport, "trolleybus_stop", 2000, 2000),
        new(InfrastructureCategoryCodes.Transport, "metro_station", 2000, 12000),
        new(InfrastructureCategoryCodes.Transport, "railway_station", 2000, 2000),
        new(InfrastructureCategoryCodes.Transport, "parking", 500, 500),
        new(InfrastructureCategoryCodes.Transport, "fuel_station", 2000, 2000),
        new(InfrastructureCategoryCodes.Transport, "service_station", 2000, 2000),

        new(InfrastructureCategoryCodes.Education, "kindergarten", 2000, 5000),
        new(InfrastructureCategoryCodes.Education, "school", 2000, 5000),
        new(InfrastructureCategoryCodes.Education, "college", 2000, 6000),
        new(InfrastructureCategoryCodes.Education, "university", 2000, 7000),

        new(InfrastructureCategoryCodes.Leisure, "park", 1000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "square", 1000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "waterfront", 1000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "fitness", 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "gym", 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "pool", 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "stadium", 2500, 6000),
        new(InfrastructureCategoryCodes.Leisure, "sports_center", 2500, 6000),
        new(InfrastructureCategoryCodes.Leisure, "museum", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "theater", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cinema", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "landmark", 3500, 8000),
        new(InfrastructureCategoryCodes.Leisure, "cafe", 1000, 3500),
        new(InfrastructureCategoryCodes.Leisure, "restaurant", 1000, 4500),
        new(InfrastructureCategoryCodes.Leisure, "bar", 1000, 4500),
        new(InfrastructureCategoryCodes.Leisure, "gallery", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "exhibition_hall", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "library", 2500, 6000),
        new(InfrastructureCategoryCodes.Leisure, "recreation_zone", 2000, 5000)
    ];

    public static readonly IReadOnlyCollection<InfrastructureFeatureDefinition> SearchFeatures =
    [
        new(InfrastructureCategoryCodes.Commerce, "store", ["supermarket", "hypermarket", "discount_store", "convenience_store"], 1000, 3000),
        new(InfrastructureCategoryCodes.Healthcare, "pharmacy", ["pharmacy"], 1000, 1000),
        new(InfrastructureCategoryCodes.Transport, "stop", ["bus_stop", "tram_stop", "trolleybus_stop"], 2000, 2000),
        new(InfrastructureCategoryCodes.Transport, "parking", ["parking"], 500, 500),
        new(InfrastructureCategoryCodes.Education, "school", ["school"], 2000, 6000),
        new(InfrastructureCategoryCodes.Education, "kindergarten", ["kindergarten"], 2000, 6000),
        new(InfrastructureCategoryCodes.Healthcare, "clinic", ["clinic"], 1500, 1500),
        new(InfrastructureCategoryCodes.Healthcare, "hospital", ["hospital"], 5000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "leisure", ["park", "square", "waterfront", "fitness", "gym", "pool", "stadium", "sports_center", "museum", "theater", "cinema", "landmark", "cafe", "restaurant", "bar", "gallery", "exhibition_hall", "library", "recreation_zone"], 1000, 10000),
        new(InfrastructureCategoryCodes.Leisure, "park", ["park", "square", "waterfront", "recreation_zone"], 1000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "fitness", ["fitness", "gym", "pool", "sports_center", "stadium"], 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "museum", ["museum", "gallery", "exhibition_hall", "library", "landmark"], 3000, 8000),
        new(InfrastructureCategoryCodes.Leisure, "theater", ["theater"], 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cinema", ["cinema"], 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cafe", ["cafe"], 1000, 3500),
        new(InfrastructureCategoryCodes.Leisure, "restaurant", ["restaurant", "bar"], 1000, 4500),
        new(InfrastructureCategoryCodes.Transport, "metro_station", ["metro_station"], 2000, 12000)
    ];

    public static InfrastructureSubcategoryDefinition? TryGetSubcategory(string subcategoryCode)
        => SupportedSubcategories.FirstOrDefault(x => x.SubcategoryCode.Equals(subcategoryCode, StringComparison.OrdinalIgnoreCase));

    public static int GetSupersetRadiusMeters()
        => SupportedSubcategories
            .Select(x => x.FallbackRadiusMeters ?? x.PrimaryRadiusMeters)
            .DefaultIfEmpty(1500)
            .Max();
}
