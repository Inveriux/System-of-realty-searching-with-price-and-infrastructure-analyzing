using System.Text;
using System.Text.RegularExpressions;
using InfrastructureService.Models;
using InfrastructureService.Services.TransportGrouping.Abstractions;

namespace InfrastructureService.Services.TransportGrouping;

public sealed class TransportStopGroupingService : ITransportStopGroupingService
{
    private static readonly string[] StopSubcategories = ["bus_stop", "tram_stop", "trolleybus_stop"];
    private static readonly Regex MultiWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex MetroAbbreviationRegex = new(@"\bст\.?\s*м\.?\b", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new("[\"'`«»()\\[\\]{}]+", RegexOptions.Compiled);
    private static readonly Regex DashRegex = new(@"[\-–—_/\\]+", RegexOptions.Compiled);
    private static readonly IReadOnlyDictionary<string, string> ReplacementMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["ул"] = "улица",
        ["ул."] = "улица",
        ["улица"] = "улица",
        ["пр"] = "проспект",
        ["пр."] = "проспект",
        ["пр-т"] = "проспект",
        ["пр-т."] = "проспект",
        ["просп"] = "проспект",
        ["просп."] = "проспект",
        ["проспект"] = "проспект",
        ["пл"] = "площадь",
        ["пл."] = "площадь",
        ["площадь"] = "площадь",
        ["ст"] = "станция",
        ["ст."] = "станция",
        ["ст.м"] = "метро",
        ["ст.м."] = "метро",
        ["ст м"] = "метро",
        ["ст. м."] = "метро",
        ["ст. м"] = "метро",
        ["метро"] = "метро"
    };
    private static readonly HashSet<string> IgnoredTokens =
    [
        "остановка",
        "остановочный",
        "пункт",
        "павильон",
        "платформа",
        "public",
        "transport",
        "stop"
    ];

    public string NormalizeStopName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var normalized = name.Trim().ToLowerInvariant();
        normalized = MetroAbbreviationRegex.Replace(normalized, " метро ");
        normalized = PunctuationRegex.Replace(normalized, " ");
        normalized = DashRegex.Replace(normalized, " ");
        normalized = MultiWhitespaceRegex.Replace(normalized, " ").Trim();

        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(static token => token.Trim('.'))
            .Select(token => ReplacementMap.TryGetValue(token, out var replacement) ? replacement : token)
            .Where(token => !IgnoredTokens.Contains(token))
            .ToArray();

        return string.Join(' ', tokens);
    }

    public IReadOnlyCollection<TransportStopGroup> Group(IReadOnlyCollection<LocationPoiSnapshotItem> items)
    {
        var stopItems = items
            .Where(IsStopPoint)
            .OrderBy(static item => item.DistanceMeters)
            .ToArray();

        if (stopItems.Length == 0)
            return [];

        var groups = new List<List<LocationPoiSnapshotItem>>();
        foreach (var item in stopItems)
        {
            var normalizedName = NormalizeStopName(item.Poi?.Name);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                groups.Add([item]);
                continue;
            }

            var existingGroup = groups.FirstOrDefault(group => CanJoinGroup(group, item, normalizedName));
            if (existingGroup is null)
            {
                groups.Add([item]);
                continue;
            }

            existingGroup.Add(item);
        }

        return groups
            .Select(BuildGroup)
            .OrderBy(static group => group.NearestDistanceMeters)
            .ToArray();
    }

    private bool CanJoinGroup(
        IReadOnlyCollection<LocationPoiSnapshotItem> group,
        LocationPoiSnapshotItem candidate,
        string candidateNormalizedName)
    {
        var firstName = NormalizeStopName(group.First().Poi?.Name);
        if (!string.Equals(firstName, candidateNormalizedName, StringComparison.Ordinal))
            return false;

        var thresholdMeters = group.Count >= 2 ? 220m : 150m;
        return group.Any(existing => CalculateDistanceMeters(
            existing.Poi!.Latitude,
            existing.Poi.Longitude,
            candidate.Poi!.Latitude,
            candidate.Poi.Longitude) <= thresholdMeters);
    }

    private TransportStopGroup BuildGroup(IReadOnlyCollection<LocationPoiSnapshotItem> group)
    {
        var representative = group.OrderBy(static item => item.DistanceMeters).First();
        var centerLatitude = group.Average(static item => item.Poi!.Latitude);
        var centerLongitude = group.Average(static item => item.Poi!.Longitude);
        var maxDistanceWithinGroup = group
            .Max(item => CalculateDistanceMeters(centerLatitude, centerLongitude, item.Poi!.Latitude, item.Poi.Longitude));
        var normalizedName = NormalizeStopName(representative.Poi?.Name);

        return new TransportStopGroup
        {
            GroupId = BuildGroupId(normalizedName, representative.PoiId),
            NormalizedName = normalizedName,
            DisplayName = representative.Poi?.Name?.Trim() ?? "Остановка",
            CenterLatitude = centerLatitude,
            CenterLongitude = centerLongitude,
            MaxDistanceWithinGroup = Math.Round(maxDistanceWithinGroup, 2),
            PointCount = group.Count,
            PlatformCount = group.Count,
            StopTypes = group
                .Select(static item => item.Poi?.SubcategoryCode)
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToArray(),
            SourcePoiIds = group.Select(static item => item.PoiId).Distinct().ToArray(),
            NearestDistanceMeters = representative.DistanceMeters,
            RepresentativeLatitude = representative.Poi!.Latitude,
            RepresentativeLongitude = representative.Poi.Longitude
        };
    }

    private static bool IsStopPoint(LocationPoiSnapshotItem item)
        => !string.IsNullOrWhiteSpace(item.Poi?.SubcategoryCode)
           && StopSubcategories.Contains(item.Poi.SubcategoryCode, StringComparer.OrdinalIgnoreCase);

    private static string BuildGroupId(string normalizedName, Guid representativePoiId)
    {
        if (string.IsNullOrWhiteSpace(normalizedName))
            return representativePoiId.ToString("N");

        var builder = new StringBuilder(normalizedName.Length);
        foreach (var ch in normalizedName)
        {
            if (char.IsLetterOrDigit(ch))
                builder.Append(ch);
        }

        return builder.Length == 0 ? representativePoiId.ToString("N") : builder.ToString();
    }

    private static decimal CalculateDistanceMeters(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double earthRadiusMeters = 6_371_000d;

        var lat1 = DegreesToRadians((double)latitude1);
        var lon1 = DegreesToRadians((double)longitude1);
        var lat2 = DegreesToRadians((double)latitude2);
        var lon2 = DegreesToRadians((double)longitude2);

        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;

        var a = Math.Pow(Math.Sin(dLat / 2), 2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(earthRadiusMeters * c);
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;
}
