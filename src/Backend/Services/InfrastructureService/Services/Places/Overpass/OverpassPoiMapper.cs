using InfrastructureService.Services.Configuration;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Places.Overpass.Abstractions;

namespace InfrastructureService.Services.Places.Overpass;

public sealed class OverpassPoiMapper : IOverpassPoiMapper
{
    public OverpassMappedPoi? Map(OverpassElement element, decimal originLatitude, decimal originLongitude)
    {
        if (!element.Latitude.HasValue || !element.Longitude.HasValue || element.Tags.Count == 0)
        {
            return null;
        }

        var mapping = TryMapTags(element.Tags);
        if (mapping is null)
        {
            return null;
        }

        var (categoryCode, subcategoryCode) = mapping.Value;

        var tags = new Dictionary<string, string>(element.Tags, StringComparer.OrdinalIgnoreCase);
        var latitude = element.Latitude.Value;
        var longitude = element.Longitude.Value;
        var rawCategory = $"{categoryCode}.{subcategoryCode}";
        var name = tags.TryGetValue("name", out var value) ? value?.Trim() ?? string.Empty : string.Empty;

        return new OverpassMappedPoi(
            categoryCode,
            subcategoryCode,
            element.Type,
            new ExternalPoiResult
            {
                ExternalId = $"{element.Type}/{element.Id}",
                Name = name,
                AddressText = BuildAddress(tags),
                Latitude = latitude,
                Longitude = longitude,
                DistanceMeters = GeoDistanceHelper.CalculateDistanceMeters(originLatitude, originLongitude, latitude, longitude),
                Source = "osm-overpass",
                RawCategory = rawCategory,
                RawCategories =
                [
                    rawCategory,
                    $"{categoryCode}:{subcategoryCode}",
                    subcategoryCode
                ],
                Tags = tags
            });
    }

    private static (string CategoryCode, string SubcategoryCode)? TryMapTags(IReadOnlyDictionary<string, string> tags)
    {
        if (IsOneOf(tags, "amenity", "pharmacy"))
            return (InfrastructureCategoryCodes.Healthcare, "pharmacy");
        if (IsOneOf(tags, "shop", "chemist"))
            return (InfrastructureCategoryCodes.Commerce, "drogerie");
        if (IsOneOf(tags, "amenity", "hospital") || IsOneOf(tags, "healthcare", "hospital"))
            return (InfrastructureCategoryCodes.Healthcare, "hospital");
        if (IsOneOf(tags, "amenity", "clinic", "doctors") || IsOneOf(tags, "healthcare", "clinic"))
            return (InfrastructureCategoryCodes.Healthcare, "clinic");

        if (IsOneOf(tags, "amenity", "school"))
            return (InfrastructureCategoryCodes.Education, "school");
        if (IsOneOf(tags, "amenity", "kindergarten"))
            return (InfrastructureCategoryCodes.Education, "kindergarten");
        if (IsOneOf(tags, "amenity", "college"))
            return (InfrastructureCategoryCodes.Education, "college");
        if (IsOneOf(tags, "amenity", "university"))
            return (InfrastructureCategoryCodes.Education, "university");

        if (IsMetroTagMatch(tags))
            return (InfrastructureCategoryCodes.Transport, "metro_station");
        if (IsOneOf(tags, "railway", "tram_stop"))
            return (InfrastructureCategoryCodes.Transport, "tram_stop");
        if (IsOneOf(tags, "highway", "bus_stop") || IsOneOf(tags, "amenity", "bus_station"))
            return (InfrastructureCategoryCodes.Transport, "bus_stop");
        if (IsOneOf(tags, "railway", "station", "halt"))
            return (InfrastructureCategoryCodes.Transport, "railway_station");
        if (IsOneOf(tags, "amenity", "parking"))
            return (InfrastructureCategoryCodes.Transport, "parking");
        if (IsOneOf(tags, "amenity", "fuel"))
            return (InfrastructureCategoryCodes.Transport, "fuel_station");
        if (IsOneOf(tags, "shop", "car_repair") || IsOneOf(tags, "craft", "car_repair") || IsOneOf(tags, "amenity", "car_wash"))
            return (InfrastructureCategoryCodes.Transport, "service_station");

        if (IsOneOf(tags, "shop", "supermarket"))
            return (InfrastructureCategoryCodes.Commerce, "supermarket");
        if (IsOneOf(tags, "shop", "convenience", "greengrocer", "bakery"))
            return (InfrastructureCategoryCodes.Commerce, "convenience_store");
        if (IsOneOf(tags, "shop", "mall", "department_store", "clothes"))
            return (InfrastructureCategoryCodes.Commerce, "mall");
        if (IsOneOf(tags, "shop", "electronics"))
            return (InfrastructureCategoryCodes.Commerce, "electronics");
        if (IsOneOf(tags, "shop", "hardware", "doityourself"))
            return (InfrastructureCategoryCodes.Commerce, "building_store");
        if (IsOneOf(tags, "shop", "hairdresser", "beauty", "cosmetics") ||
            IsOneOf(tags, "amenity", "beauty_salon", "hairdresser", "barber") ||
            IsOneOf(tags, "beauty", "hairdresser", "beautician", "nails", "spa", "massage"))
            return (InfrastructureCategoryCodes.Commerce, "beauty_service");

        if (IsOneOf(tags, "leisure", "park", "garden"))
            return (InfrastructureCategoryCodes.Leisure, "park");
        if (IsOneOf(tags, "leisure", "nature_reserve", "common", "recreation_ground", "dog_park") ||
            IsOneOf(tags, "natural", "wood", "beach", "grassland", "scrub", "heath", "wetland") ||
            IsOneOf(tags, "landuse", "forest", "village_green", "recreation_ground", "meadow") ||
            IsOneOf(tags, "boundary", "national_park", "protected_area"))
            return (InfrastructureCategoryCodes.Leisure, "recreation_zone");
        if (IsOneOf(tags, "amenity", "cinema"))
            return (InfrastructureCategoryCodes.Leisure, "cinema");
        if (IsOneOf(tags, "tourism", "museum"))
            return (InfrastructureCategoryCodes.Leisure, "museum");
        if (IsOneOf(tags, "tourism", "gallery"))
            return (InfrastructureCategoryCodes.Leisure, "gallery");
        if (IsOneOf(tags, "amenity", "theatre"))
            return (InfrastructureCategoryCodes.Leisure, "theater");
        if (IsOneOf(tags, "amenity", "library"))
            return (InfrastructureCategoryCodes.Leisure, "library");
        if (IsOneOf(tags, "amenity", "arts_centre"))
            return (InfrastructureCategoryCodes.Leisure, "exhibition_hall");
        if (IsOneOf(tags, "amenity", "cafe"))
            return (InfrastructureCategoryCodes.Leisure, "cafe");
        if (IsOneOf(tags, "amenity", "restaurant", "fast_food"))
            return (InfrastructureCategoryCodes.Leisure, "restaurant");
        if (IsOneOf(tags, "amenity", "bar"))
            return (InfrastructureCategoryCodes.Leisure, "bar");
        if (IsOneOf(tags, "leisure", "fitness_centre") || IsOneOf(tags, "sport", "fitness"))
            return (InfrastructureCategoryCodes.Leisure, "fitness");
        if (IsOneOf(tags, "tourism", "attraction", "artwork", "viewpoint") ||
            IsOneOf(tags, "historic", "memorial", "monument", "castle", "ruins") ||
            IsOneOf(tags, "amenity", "place_of_worship") ||
            IsOneOf(tags, "building", "church", "cathedral", "chapel", "mosque", "synagogue", "temple"))
            return (InfrastructureCategoryCodes.Leisure, "landmark");

        return null;
    }

    private static string? BuildAddress(IReadOnlyDictionary<string, string> tags)
    {
        if (tags.TryGetValue("addr:full", out var fullAddress) && !string.IsNullOrWhiteSpace(fullAddress))
            return fullAddress.Trim();

        var street = tags.TryGetValue("addr:street", out var streetValue) ? streetValue?.Trim() : null;
        var houseNumber = tags.TryGetValue("addr:housenumber", out var houseValue) ? houseValue?.Trim() : null;

        if (!string.IsNullOrWhiteSpace(street) && !string.IsNullOrWhiteSpace(houseNumber))
            return $"{street} {houseNumber}";

        return string.IsNullOrWhiteSpace(street) ? null : street;
    }

    private static bool IsMetroTagMatch(IReadOnlyDictionary<string, string> tags)
        => IsOneOf(tags, "railway", "subway", "subway_entrance")
           || IsOneOf(tags, "station", "subway")
           || (IsOneOf(tags, "railway", "station") && IsOneOf(tags, "station", "subway"));

    private static bool IsOneOf(IReadOnlyDictionary<string, string> tags, string key, params string[] values)
        => tags.TryGetValue(key, out var tagValue) &&
           values.Any(value => tagValue.Equals(value, StringComparison.OrdinalIgnoreCase));
}
