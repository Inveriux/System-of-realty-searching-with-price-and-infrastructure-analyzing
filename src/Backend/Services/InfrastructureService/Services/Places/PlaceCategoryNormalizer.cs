using InfrastructureService.Services.Places.Abstractions;

namespace InfrastructureService.Services.Places;

public sealed class PlaceCategoryNormalizer : IPlaceCategoryNormalizer
{
    private static readonly IReadOnlyCollection<PlaceCategoryRule> Rules =
    [
        new("raw", ["education.school", "building.school", "building.kindergarten", "building.university", "education"], "education"),
        new("raw", ["healthcare.hospital", "healthcare.pharmacy", "building.healthcare", "healthcare"], "healthcare"),
        new("raw", ["public_transport", "public_transport.", "railway", "railway."], "transport"),
        new("raw", ["commercial.supermarket", "commercial.shopping_mall", "commercial.marketplace", "building.commercial", "commercial"], "commerce"),
        new("raw", ["leisure.park", "leisure.playground", "activity.sport_club", "catering.cafe", "entertainment.cinema", "leisure", "activity", "catering"], "leisure"),

        new("amenity", ["school", "kindergarten", "college", "university"], "education"),
        new("building", ["school", "kindergarten", "university", "college"], "education"),

        new("amenity", ["hospital", "clinic", "doctors", "pharmacy", "dentist"], "healthcare"),
        new("healthcare", ["hospital", "clinic", "doctor", "pharmacy", "dentist"], "healthcare"),
        new("shop", ["medical_supply"], "healthcare"),

        new("public_transport", ["platform", "station", "stop_position"], "transport"),
        new("highway", ["bus_stop"], "transport"),
        new("railway", ["station", "halt", "tram_stop", "subway_entrance"], "transport"),
        new("amenity", ["bus_station"], "transport"),

        new("shop", ["supermarket", "convenience", "mall", "department_store", "grocery", "wholesale", "general"], "commerce"),
        new("amenity", ["marketplace"], "commerce"),
        new("building", ["retail", "supermarket", "commercial"], "commerce"),

        new("leisure", ["park", "fitness_centre", "sports_centre", "playground", "stadium", "garden"], "leisure"),
        new("amenity", ["cinema", "cafe", "theatre", "restaurant", "fast_food"], "leisure"),
        new("tourism", ["museum", "attraction"], "leisure")
    ];

    public string? Normalize(IReadOnlyDictionary<string, string> tags)
    {
        foreach (var rule in Rules)
        {
            if (tags.TryGetValue(rule.TagKey, out var value) &&
                rule.Values.Any(candidate =>
                    value.Equals(candidate, StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)))
                return rule.Category;
        }

        return null;
    }

    private sealed record PlaceCategoryRule(string TagKey, IReadOnlyCollection<string> Values, string Category);
}
