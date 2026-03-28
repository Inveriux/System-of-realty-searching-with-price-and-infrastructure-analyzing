using InfrastructureService.Services.Classification.Abstractions;
using InfrastructureService.Services.Configuration;
using InfrastructureService.Services.Places.Abstractions;

namespace InfrastructureService.Services.Classification;

public sealed class PoiClassificationService(
    PoiClassificationDictionaryProvider dictionaryProvider,
    Services.Places.Abstractions.IPlaceCategoryNormalizer placeCategoryNormalizer) : IPoiClassificationService
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly ClassificationResources _resources = dictionaryProvider.GetResources();

    public PoiClassificationResult? Classify(ExternalPoiResult poi)
    {
        var searchableText = BuildSearchableText(poi);
        if (ContainsAny(searchableText, _resources.ExcludeKeywordDictionary.ExcludeKeywords))
            return null;

        var providerTagMatch = TryClassifyFromProviderTags(poi, searchableText);
        if (providerTagMatch is not null)
            return providerTagMatch;

        var rawCategoryMatch = TryClassifyFromRawCategories(poi);
        if (rawCategoryMatch is not null)
            return rawCategoryMatch;

        var brandMatch = TryClassifyFromBrands(searchableText);
        if (brandMatch is not null)
            return brandMatch;

        var keywordMatch = TryClassifyFromKeywords(searchableText);
        if (keywordMatch is not null)
            return keywordMatch;

        var normalizationTags = new Dictionary<string, string>(poi.Tags, Comparer);
        if (!string.IsNullOrWhiteSpace(poi.RawCategory))
            normalizationTags["raw"] = poi.RawCategory!;
        if (poi.RawCategories.Count > 0)
            normalizationTags["raw_categories"] = string.Join('|', poi.RawCategories);

        var fallbackCategory = placeCategoryNormalizer.Normalize(normalizationTags);
        return string.IsNullOrWhiteSpace(fallbackCategory)
            ? null
            : new PoiClassificationResult
            {
                CategoryCode = fallbackCategory,
                SubcategoryCode = null,
                Source = "category-fallback",
                Confidence = 0.35m
            };
    }

    public Dictionary<string, string> GetDictionaryVersions() => _resources.GetVersions();

    private PoiClassificationResult? TryClassifyFromProviderTags(ExternalPoiResult poi, string searchableText)
    {
        var tags = poi.Tags;

        if (IsOneOf(tags, "shop", "supermarket", "grocery"))
            return Create(InfrastructureCategoryCodes.Commerce, "supermarket", "provider-tags", 0.96m);
        if (IsOneOf(tags, "shop", "convenience"))
            return Create(InfrastructureCategoryCodes.Commerce, "convenience_store", "provider-tags", 0.95m);
        if (IsOneOf(tags, "shop", "mall", "department_store"))
            return Create(InfrastructureCategoryCodes.Commerce, "mall", "provider-tags", 0.95m);
        if (IsOneOf(tags, "shop", "electronics"))
            return Create(InfrastructureCategoryCodes.Commerce, "electronics", "provider-tags", 0.95m);
        if (IsOneOf(tags, "shop", "furniture"))
            return Create(InfrastructureCategoryCodes.Commerce, "furniture", "provider-tags", 0.95m);
        if (IsOneOf(tags, "shop", "doityourself", "hardware", "trade"))
            return Create(InfrastructureCategoryCodes.Commerce, "building_store", "provider-tags", 0.95m);
        if (IsOneOf(tags, "shop", "chemist", "beauty"))
            return Create(InfrastructureCategoryCodes.Commerce, "drogerie", "provider-tags", 0.94m);
        if (IsOneOf(tags, "shop", "hairdresser", "cosmetics") ||
            IsOneOf(tags, "amenity", "beauty_salon", "hairdresser", "barber") ||
            IsOneOf(tags, "beauty", "hairdresser", "beautician", "nails", "spa", "massage") ||
            ContainsAny(searchableText, ["парикмахер", "барбершоп", "barbershop", "hairdresser", "beauty salon", "салон красоты", "маникюр", "nails"]))
            return Create(InfrastructureCategoryCodes.Commerce, "beauty_service", "provider-tags", 0.90m);

        if (IsOneOf(tags, "amenity", "pharmacy") || IsOneOf(tags, "healthcare", "pharmacy"))
            return Create(InfrastructureCategoryCodes.Healthcare, "pharmacy", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "clinic") || IsOneOf(tags, "healthcare", "clinic", "doctor"))
            return Create(InfrastructureCategoryCodes.Healthcare, "clinic", "provider-tags", 0.97m);
        if (IsOneOf(tags, "amenity", "hospital") || IsOneOf(tags, "healthcare", "hospital"))
            return Create(InfrastructureCategoryCodes.Healthcare, "hospital", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "dentist") || IsOneOf(tags, "healthcare", "dentist"))
            return Create(InfrastructureCategoryCodes.Healthcare, "dentistry", "provider-tags", 0.98m);
        if (Contains(searchableText, "lab") || Contains(searchableText, "лаборатория"))
            return Create(InfrastructureCategoryCodes.Healthcare, "laboratory", "provider-tags", 0.81m);

        if (IsOneOf(tags, "highway", "bus_stop") || IsOneOf(tags, "amenity", "bus_station"))
            return Create(InfrastructureCategoryCodes.Transport, "bus_stop", "provider-tags", 0.97m);
        if (IsOneOf(tags, "railway", "tram_stop"))
            return Create(InfrastructureCategoryCodes.Transport, "tram_stop", "provider-tags", 0.98m);
        if (IsMetroTagMatch(tags))
            return Create(InfrastructureCategoryCodes.Transport, "metro_station", "provider-tags", 0.99m);
        if (IsOneOf(tags, "railway", "station", "halt"))
            return Create(InfrastructureCategoryCodes.Transport, "railway_station", "provider-tags", 0.94m);
        if (IsOneOf(tags, "amenity", "parking", "parking_entrance"))
            return Create(InfrastructureCategoryCodes.Transport, "parking", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "fuel"))
            return Create(InfrastructureCategoryCodes.Transport, "fuel_station", "provider-tags", 0.96m);
        if (IsOneOf(tags, "shop", "car_repair") || IsOneOf(tags, "craft", "car_repair") || IsOneOf(tags, "amenity", "car_wash"))
            return Create(InfrastructureCategoryCodes.Transport, "service_station", "provider-tags", 0.96m);
        if (ContainsAny(searchableText, _resources.StopKeywordDictionary.StopKeywords))
        {
            if (Contains(searchableText, "трамвай") || Contains(searchableText, "tram"))
                return Create(InfrastructureCategoryCodes.Transport, "tram_stop", "provider-stop-keyword", 0.88m);
            if (Contains(searchableText, "троллейбус"))
                return Create(InfrastructureCategoryCodes.Transport, "trolleybus_stop", "provider-stop-keyword", 0.88m);
            return Create(InfrastructureCategoryCodes.Transport, "bus_stop", "provider-stop-keyword", 0.85m);
        }

        if (IsOneOf(tags, "amenity", "kindergarten") || IsOneOf(tags, "building", "kindergarten"))
            return Create(InfrastructureCategoryCodes.Education, "kindergarten", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "school") || IsOneOf(tags, "building", "school"))
            return Create(InfrastructureCategoryCodes.Education, "school", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "college") || IsOneOf(tags, "building", "college"))
            return Create(InfrastructureCategoryCodes.Education, "college", "provider-tags", 0.97m);
        if (IsOneOf(tags, "amenity", "university") || IsOneOf(tags, "building", "university"))
            return Create(InfrastructureCategoryCodes.Education, "university", "provider-tags", 0.98m);

        if (IsOneOf(tags, "leisure", "park", "garden"))
            return Create(InfrastructureCategoryCodes.Leisure, "park", "provider-tags", 0.97m);
        if (IsOneOf(tags, "leisure", "nature_reserve", "common", "recreation_ground", "dog_park") ||
            IsOneOf(tags, "natural", "wood", "beach", "grassland", "scrub", "heath", "wetland") ||
            IsOneOf(tags, "landuse", "forest", "village_green", "recreation_ground", "meadow") ||
            IsOneOf(tags, "boundary", "national_park", "protected_area"))
            return Create(InfrastructureCategoryCodes.Leisure, "recreation_zone", "provider-tags", 0.96m);
        if (IsOneOf(tags, "place", "square"))
            return Create(InfrastructureCategoryCodes.Leisure, "square", "provider-tags", 0.97m);
        if (IsOneOf(tags, "natural", "beach"))
            return Create(InfrastructureCategoryCodes.Leisure, "recreation_zone", "provider-tags", 0.96m);
        if (IsOneOf(tags, "amenity", "cinema"))
            return Create(InfrastructureCategoryCodes.Leisure, "cinema", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "cafe", "fast_food"))
            return Create(InfrastructureCategoryCodes.Leisure, "cafe", "provider-tags", 0.96m);
        if (IsOneOf(tags, "amenity", "restaurant"))
            return Create(InfrastructureCategoryCodes.Leisure, "restaurant", "provider-tags", 0.96m);
        if (IsOneOf(tags, "amenity", "bar", "pub"))
            return Create(InfrastructureCategoryCodes.Leisure, "bar", "provider-tags", 0.96m);
        if (IsOneOf(tags, "leisure", "fitness_centre"))
            return Create(InfrastructureCategoryCodes.Leisure, "fitness", "provider-tags", 0.96m);
        if (IsOneOf(tags, "leisure", "sports_centre"))
            return Create(InfrastructureCategoryCodes.Leisure, "sports_center", "provider-tags", 0.96m);
        if (IsOneOf(tags, "leisure", "swimming_pool"))
            return Create(InfrastructureCategoryCodes.Leisure, "pool", "provider-tags", 0.96m);
        if (IsOneOf(tags, "leisure", "stadium") || IsOneOf(tags, "building", "stadium"))
            return Create(InfrastructureCategoryCodes.Leisure, "stadium", "provider-tags", 0.97m);
        if (IsOneOf(tags, "building", "sports_hall"))
            return Create(InfrastructureCategoryCodes.Leisure, "gym", "provider-tags", 0.95m);
        if (IsOneOf(tags, "tourism", "museum"))
            return Create(InfrastructureCategoryCodes.Leisure, "museum", "provider-tags", 0.97m);
        if (IsOneOf(tags, "tourism", "gallery"))
            return Create(InfrastructureCategoryCodes.Leisure, "gallery", "provider-tags", 0.97m);
        if (IsOneOf(tags, "tourism", "attraction", "artwork", "viewpoint") ||
            IsOneOf(tags, "historic", "memorial", "monument", "castle", "ruins") ||
            IsOneOf(tags, "amenity", "place_of_worship") ||
            IsOneOf(tags, "building", "church", "cathedral", "chapel", "mosque", "synagogue", "temple"))
            return Create(InfrastructureCategoryCodes.Leisure, "landmark", "provider-tags", 0.96m);
        if (IsOneOf(tags, "amenity", "theatre") || IsOneOf(tags, "building", "theatre"))
            return Create(InfrastructureCategoryCodes.Leisure, "theater", "provider-tags", 0.98m);
        if (IsOneOf(tags, "amenity", "library"))
            return Create(InfrastructureCategoryCodes.Leisure, "library", "provider-tags", 0.97m);
        if (IsOneOf(tags, "amenity", "arts_centre"))
            return Create(InfrastructureCategoryCodes.Leisure, "exhibition_hall", "provider-tags", 0.95m);
        if (ContainsAny(searchableText, ["набережная", "embankment"]))
            return Create(InfrastructureCategoryCodes.Leisure, "waterfront", "provider-tags", 0.88m);

        return null;
    }

    private PoiClassificationResult? TryClassifyFromRawCategories(ExternalPoiResult poi)
    {
        foreach (var rawCategory in EnumerateRawCategories(poi))
        {
            if (rawCategory.Contains("pharmacy", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Healthcare, "pharmacy", "raw-category", 0.94m);
            if (rawCategory.Contains("hospital", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Healthcare, "hospital", "raw-category", 0.94m);
            if (rawCategory.Contains("clinic", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("medical", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Healthcare, "clinic", "raw-category", 0.92m);
            if (rawCategory.Contains("dent", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Healthcare, "dentistry", "raw-category", 0.93m);

            if (rawCategory.Contains("shopping_mall", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("mall", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Commerce, "mall", "raw-category", 0.92m);
            if (rawCategory.Contains("supermarket", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Commerce, "supermarket", "raw-category", 0.92m);
            if (rawCategory.Contains("electronics", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Commerce, "electronics", "raw-category", 0.91m);

            if (rawCategory.Contains("subway", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("metro", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Transport, "metro_station", "raw-category", 0.95m);
            if (rawCategory.Contains("railway", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("train_station", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Transport, "railway_station", "raw-category", 0.93m);
            if (rawCategory.Contains("public_transport", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Transport, "bus_stop", "raw-category", 0.89m);
            if (rawCategory.Contains("fuel", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Transport, "fuel_station", "raw-category", 0.90m);
            if (rawCategory.Contains("car_repair", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("car_service", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("car_wash", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Transport, "service_station", "raw-category", 0.90m);

            if (rawCategory.Contains("kindergarten", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Education, "kindergarten", "raw-category", 0.93m);
            if (rawCategory.Contains("school", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Education, "school", "raw-category", 0.93m);
            if (rawCategory.Contains("college", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Education, "college", "raw-category", 0.93m);
            if (rawCategory.Contains("university", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Education, "university", "raw-category", 0.93m);

            if (rawCategory.Contains("park", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "park", "raw-category", 0.91m);
            if (rawCategory.Contains("square", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "square", "raw-category", 0.91m);
            if (rawCategory.Contains("waterfront", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("embankment", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "waterfront", "raw-category", 0.90m);
            if (rawCategory.Contains("cinema", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "cinema", "raw-category", 0.91m);
            if (rawCategory.Contains("cafe", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "cafe", "raw-category", 0.91m);
            if (rawCategory.Contains("restaurant", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "restaurant", "raw-category", 0.91m);
            if (rawCategory.Contains("bar", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("pub", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "bar", "raw-category", 0.91m);
            if (rawCategory.Contains("fitness", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "fitness", "raw-category", 0.91m);
            if (rawCategory.Contains("gym", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("sports_hall", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "gym", "raw-category", 0.91m);
            if (rawCategory.Contains("pool", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("swimming", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "pool", "raw-category", 0.91m);
            if (rawCategory.Contains("stadium", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("arena", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "stadium", "raw-category", 0.91m);
            if (rawCategory.Contains("sports_centre", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("sport_club", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "sports_center", "raw-category", 0.91m);
            if (rawCategory.Contains("museum", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "museum", "raw-category", 0.91m);
            if (rawCategory.Contains("theatre", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("theater", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "theater", "raw-category", 0.91m);
            if (rawCategory.Contains("gallery", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "gallery", "raw-category", 0.91m);
            if (rawCategory.Contains("attraction", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("monument", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("memorial", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("viewpoint", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("artwork", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("castle", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("place_of_worship", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("church", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("cathedral", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("mosque", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("synagogue", StringComparison.OrdinalIgnoreCase) ||
                rawCategory.Contains("temple", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "landmark", "raw-category", 0.91m);
            if (rawCategory.Contains("exhibition", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("arts_centre", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "exhibition_hall", "raw-category", 0.91m);
            if (rawCategory.Contains("library", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "library", "raw-category", 0.91m);
            if (rawCategory.Contains("recreation", StringComparison.OrdinalIgnoreCase) || rawCategory.Contains("beach", StringComparison.OrdinalIgnoreCase))
                return Create(InfrastructureCategoryCodes.Leisure, "recreation_zone", "raw-category", 0.90m);
        }

        return null;
    }

    private PoiClassificationResult? TryClassifyFromBrands(string searchableText)
    {
        foreach (var (category, subcategory, alias) in _resources.BrandDictionary.EnumerateAliases())
        {
            if (Contains(searchableText, alias))
                return Create(category, subcategory, "brand-dictionary", 0.87m);
        }

        return null;
    }

    private PoiClassificationResult? TryClassifyFromKeywords(string searchableText)
    {
        foreach (var keyword in _resources.KeywordDictionary.Keywords)
        {
            if (Contains(searchableText, keyword.Keyword))
                return Create(keyword.Category, keyword.Subcategory, "keyword-dictionary", 0.74m);
        }

        return null;
    }

    private static IEnumerable<string> EnumerateRawCategories(ExternalPoiResult poi)
    {
        if (!string.IsNullOrWhiteSpace(poi.RawCategory))
            yield return poi.RawCategory!;

        foreach (var rawCategory in poi.RawCategories)
        {
            if (!string.IsNullOrWhiteSpace(rawCategory))
                yield return rawCategory;
        }
    }

    private static string BuildSearchableText(ExternalPoiResult poi)
    {
        var parts = new List<string>
        {
            poi.Name,
            poi.AddressText ?? string.Empty,
            poi.RawCategory ?? string.Empty,
            string.Join(' ', poi.RawCategories)
        };

        foreach (var value in poi.Tags.Values)
            parts.Add(value);

        return string.Join(' ', parts).Trim().ToLowerInvariant();
    }

    private static bool IsOneOf(IReadOnlyDictionary<string, string> tags, string key, params string[] candidates)
        => tags.TryGetValue(key, out var value) &&
           candidates.Any(candidate => value.Equals(candidate, StringComparison.OrdinalIgnoreCase));

    private static bool IsMetroTagMatch(IReadOnlyDictionary<string, string> tags)
        => IsOneOf(tags, "station", "subway")
           || IsOneOf(tags, "railway", "subway", "subway_entrance")
           || (IsOneOf(tags, "railway", "station") && (IsOneOf(tags, "subway", "yes") || IsOneOf(tags, "station", "subway")))
           || (IsOneOf(tags, "public_transport", "station", "platform", "stop_position")
               && (IsOneOf(tags, "subway", "yes") || IsOneOf(tags, "station", "subway")));

    private static bool Contains(string source, string fragment)
        => !string.IsNullOrWhiteSpace(fragment) &&
           source.Contains(fragment.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);

    private static bool ContainsAny(string source, IEnumerable<string> fragments)
        => fragments.Any(fragment => Contains(source, fragment));

    private static PoiClassificationResult Create(string categoryCode, string subcategoryCode, string source, decimal confidence)
        => new()
        {
            CategoryCode = categoryCode,
            SubcategoryCode = InfrastructureMetadata.TryGetSubcategory(subcategoryCode) is null ? null : subcategoryCode,
            Source = source,
            Confidence = confidence
        };
}
