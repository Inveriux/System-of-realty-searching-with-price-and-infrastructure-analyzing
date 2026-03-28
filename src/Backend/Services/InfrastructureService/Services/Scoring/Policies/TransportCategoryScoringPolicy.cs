using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.Configuration;
using InfrastructureService.Services.Scoring.ScoringEntities;

namespace InfrastructureService.Services.Scoring.Policies;

public sealed class TransportCategoryScoringPolicy(IMetroApplicabilityService metroApplicabilityService) : CategoryScoringPolicyBase
{
    public override string CategoryCode => "transport";

    public override CategoryScoreBreakdownDto Score(
        Location location,
        IReadOnlyCollection<InfrastructureScoringEntity> entities,
        InfrastructureDetailsDto details,
        InfrastructureScoringProfileOptions profile)
    {
        var rules = profile.Transport;
        var transportEntities = entities.OfType<TransportScoringEntity>().ToArray();
        var metroApplicable = metroApplicabilityService.IsApplicable(location);

        var bus = GetEntities(transportEntities, "bus_stop_group");
        var tram = GetEntities(transportEntities, "tram_stop_group");
        var trolley = GetEntities(transportEntities, "trolleybus_stop_group");
        var metro = GetEntities(transportEntities, "metro_station");
        var railway = GetEntities(transportEntities, "railway_station");
        var parking = GetEntities(transportEntities, "parking");
        var fuel = GetEntities(transportEntities, "fuel_station");
        var serviceStation = GetEntities(transportEntities, "service_station");

        var surfaceCandidates = bus.Cast<TransportScoringEntity>().Concat(tram).Concat(trolley).OrderBy(static entity => entity.DistanceMeters).ToArray();
        var surfaceNearest = surfaceCandidates.FirstOrDefault();
        var metroNearest = metro.FirstOrDefault();
        var railwayNearest = railway.FirstOrDefault();
        var parkingNearest = parking.FirstOrDefault();
        var fuelNearest = fuel.FirstOrDefault();
        var serviceNearest = serviceStation.FirstOrDefault();
        var surfaceModeCount = new[] { bus.Length > 0, tram.Length > 0, trolley.Length > 0 }.Count(static value => value);

        if (transportEntities.Length == 0)
        {
            return new CategoryScoreBreakdownDto
            {
                MissingImportantSubcategories = metroApplicable
                    ? ["surface_transport", "parking", "metro_or_railway"]
                    : ["surface_transport", "parking", "railway_or_surface_strength"],
                DebugInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["metro_applicable"] = metroApplicable.ToString().ToLowerInvariant()
                }
            };
        }

        var surfaceNearestRatio = surfaceNearest is null ? 0m : Math.Clamp((rules.SurfaceStopRadiusMeters - surfaceNearest.DistanceMeters) / rules.SurfaceStopRadiusMeters, 0m, 1m);
        var rawSurfaceUsefulCount = Math.Min(bus.Length, rules.BusStopGroupUsefulCountCap)
                                    + Math.Min(tram.Length, rules.TramStopGroupUsefulCountCap)
                                    + Math.Min(trolley.Length, rules.TrolleybusStopGroupUsefulCountCap);
        var surfaceCountRatio = Math.Clamp(rawSurfaceUsefulCount / Math.Max(rules.SurfaceUsefulCountTarget, 1m), 0m, 1m);
        var surfaceModeRatio = Math.Clamp(surfaceModeCount / 3m, 0m, 1m);

        var surfaceStopsScore =
            (surfaceNearestRatio * rules.SurfaceNearestWeight +
             surfaceCountRatio * rules.SurfaceCountSaturationWeight +
             surfaceModeRatio * rules.SurfaceModeRichnessWeight) * rules.SurfaceStopsWeight;

        var metroScore = metroApplicable && metroNearest is not null
            ? CalculateProximityContribution(metroNearest.DistanceMeters, rules.MetroRadiusMeters, rules.MetroWeight)
            : 0m;
        var railwayScore = railwayNearest is not null
            ? CalculateProximityContribution(railwayNearest.DistanceMeters, rules.RailwayRadiusMeters, rules.RailwayWeight)
            : 0m;
        var parkingScore = parkingNearest is not null
            ? CalculateProximityContribution(parkingNearest.DistanceMeters, rules.ParkingRadiusMeters, rules.ParkingWeight)
            : 0m;
        var fuelScore = fuelNearest is not null
            ? CalculateProximityContribution(fuelNearest.DistanceMeters, rules.FuelRadiusMeters, rules.FuelWeight)
            : 0m;
        var serviceStationScore = serviceNearest is not null
            ? CalculateProximityContribution(serviceNearest.DistanceMeters, rules.ServiceStationRadiusMeters, rules.ServiceStationWeight)
            : 0m;

        var diversityDenominator = metroApplicable ? 4m : 3m;
        var diversityPresent =
            (surfaceNearest is not null ? 1 : 0) +
            (parkingNearest is not null ? 1 : 0) +
            (railwayNearest is not null ? 1 : 0) +
            (metroApplicable && metroNearest is not null ? 1 : 0);
        var diversityScore = (diversityPresent / diversityDenominator) * rules.DiversityWeightTransport;

        var completenessAchieved =
            surfaceNearest is not null &&
            parkingNearest is not null &&
            surfaceNearest.DistanceMeters <= rules.CompletenessMaxDistanceMeters &&
            parkingNearest.DistanceMeters <= rules.CompletenessMaxDistanceMeters &&
            (
                metroApplicable
                    ? (metroNearest is not null && metroNearest.DistanceMeters <= rules.CompletenessMaxDistanceMeters)
                      || (railwayNearest is not null && railwayNearest.DistanceMeters <= rules.CompletenessMaxDistanceMeters)
                    : (railwayNearest is not null && railwayNearest.DistanceMeters <= rules.CompletenessMaxDistanceMeters)
                      || surfaceModeCount >= 2
            );
        var completenessBonus = completenessAchieved ? rules.CompletenessWeightTransport : 0m;

        var rawScore = surfaceStopsScore + metroScore + railwayScore + parkingScore + fuelScore + serviceStationScore + diversityScore + completenessBonus;
        var cappedScore = ApplyTransportCap(rawScore, rawSurfaceUsefulCount, metroApplicable, surfaceNearest is not null, parkingNearest is not null, railwayNearest is not null, metroNearest is not null, surfaceModeCount, rules, out var appliedCap, out var capReason);

        var groupedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["bus_stop_group"] = bus.Length,
            ["tram_stop_group"] = tram.Length,
            ["trolleybus_stop_group"] = trolley.Length,
            ["metro_station"] = metro.Length,
            ["railway_station"] = railway.Length,
            ["parking"] = parking.Length,
            ["fuel_station"] = fuel.Length,
            ["service_station"] = serviceStation.Length
        };

        var presentSubcategories = new List<string>();
        if (bus.Length > 0) presentSubcategories.Add("bus_stop_group");
        if (tram.Length > 0) presentSubcategories.Add("tram_stop_group");
        if (trolley.Length > 0) presentSubcategories.Add("trolleybus_stop_group");
        if (metro.Length > 0) presentSubcategories.Add("metro_station");
        if (railway.Length > 0) presentSubcategories.Add("railway_station");
        if (parking.Length > 0) presentSubcategories.Add("parking");
        if (fuel.Length > 0) presentSubcategories.Add("fuel_station");
        if (serviceStation.Length > 0) presentSubcategories.Add("service_station");

        var missingBlocks = new List<string>();
        if (surfaceNearest is null) missingBlocks.Add("surface_transport");
        if (parkingNearest is null) missingBlocks.Add("parking");
        if (metroApplicable && metroNearest is null) missingBlocks.Add("metro");
        if (railwayNearest is null) missingBlocks.Add("railway");

        var scoreBySubcategory = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["surface_transport"] = Round(surfaceStopsScore),
            ["metro_station"] = Round(metroScore),
            ["railway_station"] = Round(railwayScore),
            ["parking"] = Round(parkingScore),
            ["fuel_station"] = Round(fuelScore),
            ["service_station"] = Round(serviceStationScore)
        };

        return new CategoryScoreBreakdownDto
        {
            Score = Round(cappedScore),
            ProximityScore = Round(surfaceStopsScore + metroScore + railwayScore + parkingScore + fuelScore + serviceStationScore),
            DensityScore = Round(surfaceCountRatio * 20m),
            DiversityScore = Round(diversityScore),
            CompletenessBonus = Round(completenessBonus),
            AppliedCap = appliedCap,
            AppliedCapReason = capReason,
            CompletenessAchieved = completenessAchieved,
            PresentSubcategories = presentSubcategories.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingImportantSubcategories = missingBlocks,
            GroupedCounts = groupedCounts,
            ScoreBySubcategory = scoreBySubcategory,
            DebugInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["filtered_address_like_poi_count"] = details.FilteredAddressLikePoiCount.ToString(),
                ["excluded_poi_reasons"] = string.Join(", ", details.ExcludedPoiReasons.Select(static pair => $"{pair.Key}:{pair.Value}")),
                ["metro_applicable"] = metroApplicable.ToString().ToLowerInvariant(),
                ["surface_stop_groups_count"] = surfaceCandidates.Length.ToString(),
                ["surface_modes_present"] = surfaceModeCount.ToString(),
                ["surface_stops_score"] = Round(surfaceStopsScore).ToString("0.##"),
                ["surface_strength_level"] = GetSurfaceStrengthLevel(rawSurfaceUsefulCount).ToString().ToLowerInvariant(),
                ["surface_useful_count"] = rawSurfaceUsefulCount.ToString(),
                ["raw_stop_poi_before_grouping"] = details.TransportDetails.StopPlatformsCountPrimary.ToString(),
                ["found_fuel_station"] = (fuelNearest is not null).ToString().ToLowerInvariant(),
                ["found_service_station"] = (serviceNearest is not null).ToString().ToLowerInvariant(),
                ["transport_applied_cap"] = appliedCap?.ToString("0.##") ?? "none",
                ["missing_transport_blocks"] = string.Join(", ", missingBlocks),
                ["diversity_ratio"] = $"{diversityPresent}/{diversityDenominator}",
                ["found_metro"] = (metroNearest is not null).ToString().ToLowerInvariant(),
                ["found_parking"] = (parkingNearest is not null).ToString().ToLowerInvariant(),
                ["found_railway"] = (railwayNearest is not null).ToString().ToLowerInvariant()
            }
        };
    }

    private static TransportScoringEntity[] GetEntities(IEnumerable<TransportScoringEntity> entities, string subcategoryCode)
        => entities
            .Where(entity => entity.SubcategoryCode.Equals(subcategoryCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(static entity => entity.DistanceMeters)
            .ToArray();

    private static decimal ApplyTransportCap(
        decimal rawScore,
        int rawSurfaceUsefulCount,
        bool metroApplicable,
        bool hasSurface,
        bool hasParking,
        bool hasRailway,
        bool hasMetro,
        int surfaceModeCount,
        TransportScoringRulesOptions rules,
        out decimal? appliedCap,
        out string? capReason)
    {
        appliedCap = null;
        capReason = null;
        var surfaceStrengthLevel = hasSurface
            ? GetSurfaceStrengthLevel(rawSurfaceUsefulCount)
            : SurfaceStrengthLevel.None;

        if (!hasSurface)
        {
            appliedCap = 45m;
            capReason = "missing_surface_transport";
        }
        else
        {
            appliedCap = DetermineCap(metroApplicable, hasParking, hasRailway, hasMetro, surfaceModeCount, surfaceStrengthLevel, rules);
            capReason = DetermineCapReason(metroApplicable, hasParking, hasRailway, hasMetro, surfaceStrengthLevel);
        }

        return appliedCap.HasValue ? Math.Min(rawScore, appliedCap.Value) : rawScore;
    }

    private static decimal DetermineCap(
        bool metroApplicable,
        bool hasParking,
        bool hasRailway,
        bool hasMetro,
        int surfaceModeCount,
        SurfaceStrengthLevel surfaceStrengthLevel,
        TransportScoringRulesOptions rules)
    {
        var cap = surfaceStrengthLevel switch
        {
            SurfaceStrengthLevel.Weak => 68m,
            SurfaceStrengthLevel.Medium => 75m,
            SurfaceStrengthLevel.Strong => 85m,
            SurfaceStrengthLevel.Excellent => 90m,
            _ => 45m
        };

        if (hasParking)
        {
            cap = surfaceStrengthLevel switch
            {
                SurfaceStrengthLevel.Weak => Math.Max(cap, 74m),
                SurfaceStrengthLevel.Medium => Math.Max(cap, 82m),
                SurfaceStrengthLevel.Strong => Math.Max(cap, 92m),
                SurfaceStrengthLevel.Excellent => Math.Max(cap, 95m),
                _ => cap
            };
        }

        if (hasRailway)
        {
            cap = surfaceStrengthLevel switch
            {
                SurfaceStrengthLevel.Weak => Math.Max(cap, 78m),
                SurfaceStrengthLevel.Medium => Math.Max(cap, 86m),
                SurfaceStrengthLevel.Strong => Math.Max(cap, 97m),
                SurfaceStrengthLevel.Excellent => Math.Max(cap, 100m),
                _ => cap
            };
        }

        if (surfaceModeCount >= 3 && surfaceStrengthLevel >= SurfaceStrengthLevel.Strong)
        {
            cap = Math.Max(cap, hasParking ? 94m : 88m);
        }

        if (metroApplicable && hasMetro)
        {
            cap = Math.Max(cap, hasParking ? rules.SurfacePlusParkingAndMetroCap : 96m);
        }

        if (hasParking)
        {
            cap = Math.Min(cap, hasRailway ? rules.SurfacePlusParkingAndRailwayCap : rules.SurfacePlusParkingCap);
        }
        else if (!hasRailway && (!metroApplicable || !hasMetro))
        {
            cap = Math.Min(cap, rules.SurfaceOnlyCap);
        }

        if (!metroApplicable &&
            hasParking &&
            surfaceStrengthLevel >= SurfaceStrengthLevel.Strong &&
            (hasRailway || surfaceModeCount >= 2))
        {
            cap = Math.Max(cap, rules.NonMetroMaxCapWithSurfaceAndParking);
        }

        if (hasRailway && hasParking && !metroApplicable)
        {
            cap = Math.Max(cap, 100m);
        }

        return Math.Min(cap, 100m);
    }

    private static string DetermineCapReason(
        bool metroApplicable,
        bool hasParking,
        bool hasRailway,
        bool hasMetro,
        SurfaceStrengthLevel surfaceStrengthLevel)
    {
        if (metroApplicable && hasMetro && hasParking)
        {
            return "surface_plus_parking_plus_metro";
        }

        if (hasRailway && hasParking)
        {
            return metroApplicable
                ? "surface_plus_parking_plus_railway"
                : "surface_plus_parking_plus_railway_non_metro_city";
        }

        if (hasRailway)
        {
            return "surface_plus_railway";
        }

        if (hasParking)
        {
            return "surface_plus_parking";
        }

        return surfaceStrengthLevel switch
        {
            SurfaceStrengthLevel.Weak => "surface_only_weak",
            SurfaceStrengthLevel.Medium => "surface_only_medium",
            SurfaceStrengthLevel.Strong => "surface_only_strong",
            SurfaceStrengthLevel.Excellent => "surface_only_excellent",
            _ => "surface_only"
        };
    }

    private static SurfaceStrengthLevel GetSurfaceStrengthLevel(int rawSurfaceUsefulCount)
        => rawSurfaceUsefulCount switch
        {
            <= 0 => SurfaceStrengthLevel.None,
            <= 1 => SurfaceStrengthLevel.Weak,
            <= 3 => SurfaceStrengthLevel.Medium,
            <= 5 => SurfaceStrengthLevel.Strong,
            _ => SurfaceStrengthLevel.Excellent
        };

    private enum SurfaceStrengthLevel
    {
        None = 0,
        Weak = 1,
        Medium = 2,
        Strong = 3,
        Excellent = 4
    }
}
