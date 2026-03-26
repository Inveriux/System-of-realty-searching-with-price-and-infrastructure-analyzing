using System.Text.RegularExpressions;
using InfrastructureService.Services.Classification;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Sanitization.Abstractions;

namespace InfrastructureService.Services.Sanitization;

public sealed partial class PoiSanitizationService(PoiClassificationDictionaryProvider dictionaryProvider) : IPoiSanitizationService
{
    private static readonly string[] StrongPoiTagKeys = ["shop", "amenity", "healthcare", "leisure", "tourism", "public_transport"];
    private static readonly string[] StrongPoiRailwayValues = ["station", "halt", "tram_stop"];
    private static readonly string[] StrongPoiHighwayValues = ["bus_stop"];
    private static readonly string[] StrongPoiBuildingValues = ["school", "college", "university", "kindergarten", "hospital", "stadium"];
    private static readonly string[] AddressLikeKeywords =
    [
        "беларусь", "минск", "район", "микрорайон", "улица", "ул.", "переулок", "пер.", "проспект", "пр-т", "шоссе", "бульвар", "дорога", "аллея", "площадь"
    ];

    private readonly ClassificationResources _resources = dictionaryProvider.GetResources();

    public PoiSanitizationResult Evaluate(ExternalPoiResult poi)
    {
        var text = $"{poi.Name} {poi.AddressText}".Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(text))
        {
            return new PoiSanitizationResult();
        }

        var strongPoiSignals = CalculatePoiConfidence(poi, text);
        var addressLikeness = CalculateAddressLikeness(poi, text);
        var shouldExclude = addressLikeness >= 0.75m && strongPoiSignals <= 0.35m;

        return new PoiSanitizationResult
        {
            ShouldExclude = shouldExclude,
            Reason = shouldExclude ? "address_like_without_poi_semantics" : null,
            AddressLikenessScore = addressLikeness,
            PoiConfidenceScore = strongPoiSignals
        };
    }

    private decimal CalculatePoiConfidence(ExternalPoiResult poi, string text)
    {
        decimal score = 0m;

        if (poi.Tags.Keys.Any(key => StrongPoiTagKeys.Contains(key, StringComparer.OrdinalIgnoreCase)))
        {
            score += 0.65m;
        }

        if (poi.Tags.TryGetValue("railway", out var railwayValue) && StrongPoiRailwayValues.Contains(railwayValue, StringComparer.OrdinalIgnoreCase))
        {
            score += 0.55m;
        }

        if (poi.Tags.TryGetValue("highway", out var highwayValue) && StrongPoiHighwayValues.Contains(highwayValue, StringComparer.OrdinalIgnoreCase))
        {
            score += 0.55m;
        }

        if (poi.Tags.TryGetValue("building", out var buildingValue) && StrongPoiBuildingValues.Contains(buildingValue, StringComparer.OrdinalIgnoreCase))
        {
            score += 0.40m;
        }

        if (_resources.BrandDictionary.EnumerateAliases().Any(alias => text.Contains(alias.Alias, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.55m;
        }

        if (!string.IsNullOrWhiteSpace(poi.RawCategory)
            && !AddressCategoryRegex().IsMatch(poi.RawCategory)
            && (poi.RawCategory.Contains("shop", StringComparison.OrdinalIgnoreCase)
                || poi.RawCategory.Contains("amenity", StringComparison.OrdinalIgnoreCase)
                || poi.RawCategory.Contains("tourism", StringComparison.OrdinalIgnoreCase)
                || poi.RawCategory.Contains("leisure", StringComparison.OrdinalIgnoreCase)
                || poi.RawCategory.Contains("public_transport", StringComparison.OrdinalIgnoreCase)
                || poi.RawCategory.Contains("railway", StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.30m;
        }

        return Math.Clamp(score, 0m, 1m);
    }

    private static decimal CalculateAddressLikeness(ExternalPoiResult poi, string text)
    {
        decimal score = 0m;

        if (PostcodeRegex().IsMatch(text))
        {
            score += 0.35m;
        }

        if (text.Contains(','))
        {
            score += 0.10m;
        }

        if (AddressLikeKeywords.Count(text.Contains) >= 2)
        {
            score += 0.35m;
        }

        if (AddressNameRegex().IsMatch(text))
        {
            score += 0.35m;
        }

        if (!string.IsNullOrWhiteSpace(poi.RawCategory) && AddressCategoryRegex().IsMatch(poi.RawCategory))
        {
            score += 0.30m;
        }

        if (poi.RawCategories.Any(category => AddressCategoryRegex().IsMatch(category)))
        {
            score += 0.20m;
        }

        if (poi.Tags.Keys.Any(key => key.StartsWith("addr:", StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.25m;
        }

        if (poi.Tags.TryGetValue("place", out var placeValue)
            && (placeValue.Contains("neighbourhood", StringComparison.OrdinalIgnoreCase)
                || placeValue.Contains("suburb", StringComparison.OrdinalIgnoreCase)
                || placeValue.Contains("quarter", StringComparison.OrdinalIgnoreCase)
                || placeValue.Contains("district", StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.35m;
        }

        if (poi.Tags.TryGetValue("highway", out var highwayValue)
            && (highwayValue.Contains("residential", StringComparison.OrdinalIgnoreCase)
                || highwayValue.Contains("service", StringComparison.OrdinalIgnoreCase)
                || highwayValue.Contains("road", StringComparison.OrdinalIgnoreCase)
                || highwayValue.Contains("living_street", StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.35m;
        }

        return Math.Clamp(score, 0m, 1m);
    }

    [GeneratedRegex(@"\b22\d{4}\b|\b\d{6}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex PostcodeRegex();

    [GeneratedRegex(@"(^|\s)(беларусь.*(улица|ул\.|переулок|пер\.|проспект|пр-т|шоссе|бульвар))|(^|\s)(\d+-?[йя]|[0-9]+)\s*(переулок|пер\.)|(^|\s)(улица|ул\.|переулок|пер\.|проспект|пр-т|шоссе|бульвар|район|микрорайон)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex AddressNameRegex();

    [GeneratedRegex(@"street|address|house|road|residential|service|living_street|neighbourhood|suburb|district|quarter|place", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex AddressCategoryRegex();
}
