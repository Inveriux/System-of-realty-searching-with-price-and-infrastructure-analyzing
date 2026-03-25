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

        new(InfrastructureCategoryCodes.Healthcare, "pharmacy", 1500, 5000),
        new(InfrastructureCategoryCodes.Healthcare, "clinic", 2000, 5000),
        new(InfrastructureCategoryCodes.Healthcare, "hospital", 3000, 10000),
        new(InfrastructureCategoryCodes.Healthcare, "dentistry", 2000, 5000),
        new(InfrastructureCategoryCodes.Healthcare, "laboratory", 2000, 5000),

        new(InfrastructureCategoryCodes.Transport, "bus_stop", 800, 2000),
        new(InfrastructureCategoryCodes.Transport, "tram_stop", 800, 2000),
        new(InfrastructureCategoryCodes.Transport, "trolleybus_stop", 800, 2000),
        new(InfrastructureCategoryCodes.Transport, "metro_station", 3000, 7000),
        new(InfrastructureCategoryCodes.Transport, "railway_station", 800, 2000),
        new(InfrastructureCategoryCodes.Transport, "parking", 1000, 3000),

        new(InfrastructureCategoryCodes.Education, "kindergarten", 2000, 5000),
        new(InfrastructureCategoryCodes.Education, "school", 2000, 5000),
        new(InfrastructureCategoryCodes.Education, "college", 2000, 5000),
        new(InfrastructureCategoryCodes.Education, "university", 2000, 5000),

        new(InfrastructureCategoryCodes.Leisure, "park", 1500, 4000),
        new(InfrastructureCategoryCodes.Leisure, "square", 1500, 4000),
        new(InfrastructureCategoryCodes.Leisure, "waterfront", 1500, 4000),
        new(InfrastructureCategoryCodes.Leisure, "fitness", 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "gym", 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "pool", 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "stadium", 2500, 6000),
        new(InfrastructureCategoryCodes.Leisure, "sports_center", 2500, 6000),
        new(InfrastructureCategoryCodes.Leisure, "museum", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "theater", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cinema", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cafe", 1200, 3000),
        new(InfrastructureCategoryCodes.Leisure, "restaurant", 1500, 4000),
        new(InfrastructureCategoryCodes.Leisure, "bar", 1500, 4000),
        new(InfrastructureCategoryCodes.Leisure, "gallery", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "exhibition_hall", 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "library", 2500, 6000),
        new(InfrastructureCategoryCodes.Leisure, "recreation_zone", 2000, 5000)
    ];

    public static readonly IReadOnlyCollection<InfrastructureFeatureDefinition> SearchFeatures =
    [
        new(InfrastructureCategoryCodes.Commerce, "store", ["supermarket", "hypermarket", "discount_store", "convenience_store"], 1000, 3000),
        new(InfrastructureCategoryCodes.Healthcare, "pharmacy", ["pharmacy"], 1500, 5000),
        new(InfrastructureCategoryCodes.Transport, "stop", ["bus_stop", "tram_stop", "trolleybus_stop"], 800, 2000),
        new(InfrastructureCategoryCodes.Transport, "parking", ["parking"], 1000, 3000),
        new(InfrastructureCategoryCodes.Education, "school", ["school"], 2000, 5000),
        new(InfrastructureCategoryCodes.Education, "kindergarten", ["kindergarten"], 2000, 5000),
        new(InfrastructureCategoryCodes.Healthcare, "clinic", ["clinic"], 2000, 5000),
        new(InfrastructureCategoryCodes.Healthcare, "hospital", ["hospital"], 3000, 10000),
        new(InfrastructureCategoryCodes.Leisure, "leisure", ["park", "square", "waterfront", "fitness", "gym", "pool", "stadium", "sports_center", "museum", "theater", "cinema", "cafe", "restaurant", "bar", "gallery", "exhibition_hall", "library", "recreation_zone"], 1500, 7000),
        new(InfrastructureCategoryCodes.Leisure, "park", ["park", "square", "waterfront", "recreation_zone"], 1500, 4000),
        new(InfrastructureCategoryCodes.Leisure, "fitness", ["fitness", "gym", "pool", "sports_center", "stadium"], 2000, 5000),
        new(InfrastructureCategoryCodes.Leisure, "museum", ["museum", "gallery", "exhibition_hall", "library"], 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "theater", ["theater"], 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cinema", ["cinema"], 3000, 7000),
        new(InfrastructureCategoryCodes.Leisure, "cafe", ["cafe"], 1200, 3000),
        new(InfrastructureCategoryCodes.Leisure, "restaurant", ["restaurant", "bar"], 1500, 4000),
        new(InfrastructureCategoryCodes.Transport, "metro_station", ["metro_station"], 3000, 7000)
    ];

    public static InfrastructureSubcategoryDefinition? TryGetSubcategory(string subcategoryCode)
        => SupportedSubcategories.FirstOrDefault(x => x.SubcategoryCode.Equals(subcategoryCode, StringComparison.OrdinalIgnoreCase));

    public static int GetSupersetRadiusMeters()
        => SupportedSubcategories
            .Select(x => x.FallbackRadiusMeters ?? x.PrimaryRadiusMeters)
            .DefaultIfEmpty(1500)
            .Max();
}
