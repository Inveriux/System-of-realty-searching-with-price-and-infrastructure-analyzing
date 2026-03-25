using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Configuration;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Scoring.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace InfrastructureService.Services.Scoring;

public sealed class InfrastructureScoringService(IOptions<InfrastructureScoringOptions> options) : IInfrastructureScoringService
{
    private static readonly IReadOnlyDictionary<string, decimal> CategoryWeights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["education"] = 1.10m,
        ["healthcare"] = 1.05m,
        ["transport"] = 1.20m,
        ["commerce"] = 0.95m,
        ["leisure"] = 0.85m
    };

    private static readonly IReadOnlyDictionary<string, decimal> TargetCounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["education"] = 5m,
        ["healthcare"] = 6m,
        ["transport"] = 8m,
        ["commerce"] = 10m,
        ["leisure"] = 7m
    };

    private static readonly IReadOnlyDictionary<string, decimal> TargetSubcategoryDiversity = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["education"] = 3m,
        ["healthcare"] = 4m,
        ["transport"] = 4m,
        ["commerce"] = 5m,
        ["leisure"] = 6m
    };

    public InfrastructureScore Calculate(Location location, LocationPoiSnapshot snapshot, InfrastructureDetailsDto details)
    {
        var primaryItems = snapshot.Items
            .Where(x => x.Poi?.Category is not null)
            .Where(IsInsidePrimaryRadius)
            .ToArray();

        var grouped = primaryItems
            .GroupBy(x => x.Poi!.Category!.Name)
            .ToDictionary(static x => x.Key, x => x.OrderBy(static item => item.DistanceMeters).ToArray(), StringComparer.OrdinalIgnoreCase);
        var subcategoryDiversity = primaryItems
            .Where(static x => !string.IsNullOrWhiteSpace(x.Poi?.SubcategoryCode))
            .GroupBy(x => x.Poi!.Category!.Name)
            .ToDictionary(
                static x => x.Key,
                x => x.Select(item => item.Poi!.SubcategoryCode!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                StringComparer.OrdinalIgnoreCase);

        var education = CalculateCategoryScore(grouped, subcategoryDiversity, "education");
        var healthcare = CalculateCategoryScore(grouped, subcategoryDiversity, "healthcare");
        var transport = CalculateTransportScore(details, subcategoryDiversity);
        var commerce = CalculateCategoryScore(grouped, subcategoryDiversity, "commerce");
        var leisure = CalculateLeisureScore(details, grouped, subcategoryDiversity);
        var total = CalculateTotalScore(education, healthcare, transport, commerce, leisure);

        var searchFeatures = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["education_score"] = education,
            ["healthcare_score"] = healthcare,
            ["transport_score"] = transport,
            ["commerce_score"] = commerce,
            ["leisure_score"] = leisure,
            ["store_count_primary"] = details.SearchFeatures.StoreCountPrimary,
            ["pharmacy_count_primary"] = details.SearchFeatures.PharmacyCountPrimary,
            ["stops_count_primary"] = details.SearchFeatures.StopsCountPrimary,
            ["stop_groups_count_primary"] = details.SearchFeatures.StopGroupsCountPrimary,
            ["stop_platforms_count_primary"] = details.SearchFeatures.StopPlatformsCountPrimary,
            ["parking_count_primary"] = details.SearchFeatures.ParkingCountPrimary,
            ["school_count_primary"] = details.SearchFeatures.SchoolCountPrimary,
            ["kindergarten_count_primary"] = details.SearchFeatures.KindergartenCountPrimary,
            ["clinic_count_primary"] = details.SearchFeatures.ClinicCountPrimary,
            ["hospital_count_primary"] = details.SearchFeatures.HospitalCountPrimary,
            ["leisure_count_primary"] = details.SearchFeatures.LeisureCountPrimary,
            ["parks_count_primary"] = details.SearchFeatures.ParksCountPrimary,
            ["fitness_count_primary"] = details.SearchFeatures.FitnessCountPrimary,
            ["museums_count_primary"] = details.SearchFeatures.MuseumsCountPrimary,
            ["theaters_count_primary"] = details.SearchFeatures.TheatersCountPrimary,
            ["cinemas_count_primary"] = details.SearchFeatures.CinemasCountPrimary,
            ["cafes_count_primary"] = details.SearchFeatures.CafesCountPrimary,
            ["restaurants_count_primary"] = details.SearchFeatures.RestaurantsCountPrimary,
            ["metro_found_in_primary_radius"] = details.SearchFeatures.MetroFoundInPrimaryRadius ? 1m : 0m,
            ["education_subcategory_diversity_primary"] = GetSubcategoryDiversity(subcategoryDiversity, "education"),
            ["healthcare_subcategory_diversity_primary"] = GetSubcategoryDiversity(subcategoryDiversity, "healthcare"),
            ["transport_subcategory_diversity_primary"] = GetSubcategoryDiversity(subcategoryDiversity, "transport"),
            ["commerce_subcategory_diversity_primary"] = GetSubcategoryDiversity(subcategoryDiversity, "commerce"),
            ["leisure_subcategory_diversity_primary"] = GetSubcategoryDiversity(subcategoryDiversity, "leisure")
        };

        foreach (var (category, items) in grouped)
        {
            searchFeatures[$"{category}_count"] = items.Length;
            searchFeatures[$"{category}_nearest_m"] = items.Length == 0 ? 0m : items[0].DistanceMeters;
        }

        AddIfValue(searchFeatures, "nearest_store_walk_minutes", details.SearchFeatures.NearestStoreWalkMinutes);
        AddIfValue(searchFeatures, "nearest_pharmacy_walk_minutes", details.SearchFeatures.NearestPharmacyWalkMinutes);
        AddIfValue(searchFeatures, "nearest_stop_walk_minutes", details.SearchFeatures.NearestStopWalkMinutes);
        AddIfValue(searchFeatures, "nearest_stop_group_walk_minutes", details.SearchFeatures.NearestStopGroupWalkMinutes);
        AddIfValue(searchFeatures, "nearest_stop_group_distance_m", details.SearchFeatures.NearestStopGroupDistanceM);
        AddIfValue(searchFeatures, "nearest_parking_walk_minutes", details.SearchFeatures.NearestParkingWalkMinutes);
        AddIfValue(searchFeatures, "nearest_school_walk_minutes", details.SearchFeatures.NearestSchoolWalkMinutes);
        AddIfValue(searchFeatures, "nearest_kindergarten_walk_minutes", details.SearchFeatures.NearestKindergartenWalkMinutes);
        AddIfValue(searchFeatures, "nearest_clinic_drive_minutes", details.SearchFeatures.NearestClinicDriveMinutes);
        AddIfValue(searchFeatures, "nearest_hospital_drive_minutes", details.SearchFeatures.NearestHospitalDriveMinutes);
        AddIfValue(searchFeatures, "nearest_metro_walk_minutes", details.SearchFeatures.NearestMetroWalkMinutes);
        AddIfValue(searchFeatures, "nearest_park_walk_minutes", details.SearchFeatures.NearestParkWalkMinutes);
        AddIfValue(searchFeatures, "nearest_park_distance_m", details.SearchFeatures.NearestParkDistanceM);
        AddIfValue(searchFeatures, "nearest_fitness_walk_minutes", details.SearchFeatures.NearestFitnessWalkMinutes);
        AddIfValue(searchFeatures, "nearest_fitness_drive_minutes", details.SearchFeatures.NearestFitnessDriveMinutes);
        AddIfValue(searchFeatures, "nearest_fitness_distance_m", details.SearchFeatures.NearestFitnessDistanceM);
        AddIfValue(searchFeatures, "nearest_museum_walk_minutes", details.SearchFeatures.NearestMuseumWalkMinutes);
        AddIfValue(searchFeatures, "nearest_museum_drive_minutes", details.SearchFeatures.NearestMuseumDriveMinutes);
        AddIfValue(searchFeatures, "nearest_museum_distance_m", details.SearchFeatures.NearestMuseumDistanceM);
        AddIfValue(searchFeatures, "nearest_theater_walk_minutes", details.SearchFeatures.NearestTheaterWalkMinutes);
        AddIfValue(searchFeatures, "nearest_theater_drive_minutes", details.SearchFeatures.NearestTheaterDriveMinutes);
        AddIfValue(searchFeatures, "nearest_theater_distance_m", details.SearchFeatures.NearestTheaterDistanceM);
        AddIfValue(searchFeatures, "nearest_cinema_walk_minutes", details.SearchFeatures.NearestCinemaWalkMinutes);
        AddIfValue(searchFeatures, "nearest_cinema_drive_minutes", details.SearchFeatures.NearestCinemaDriveMinutes);
        AddIfValue(searchFeatures, "nearest_cinema_distance_m", details.SearchFeatures.NearestCinemaDistanceM);

        return new InfrastructureScore
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SnapshotId = snapshot.Id,
            TotalScore = total,
            EducationScore = education,
            HealthcareScore = healthcare,
            TransportScore = transport,
            CommerceScore = commerce,
            LeisureScore = leisure,
            PoiCount = snapshot.Items.Count,
            CalculatedAtUtc = DateTime.UtcNow,
            SearchFeaturesJson = JsonSerializer.Serialize(searchFeatures),
            DetailedSummaryJson = JsonSerializer.Serialize(details)
        };
    }

    private decimal CalculateCategoryScore(
        IReadOnlyDictionary<string, LocationPoiSnapshotItem[]> grouped,
        IReadOnlyDictionary<string, int> subcategoryDiversity,
        string category)
    {
        if (!grouped.TryGetValue(category, out var items) || items.Length == 0)
            return 0m;

        var radiusMeters = Math.Max(100m, options.Value.DefaultRadiusMeters);
        var nearest = items[0].DistanceMeters;
        var nearestScore = Math.Clamp((radiusMeters - nearest) / radiusMeters, 0m, 1m) * 55m;

        var targetCount = TargetCounts.TryGetValue(category, out var countTarget) ? countTarget : 6m;
        var densityRatio = Math.Min(items.Length / targetCount, 1m);
        var densityScore = densityRatio * 30m;
        var diversityTarget = TargetSubcategoryDiversity.TryGetValue(category, out var diversityTargetValue) ? diversityTargetValue : 3m;
        var diversityRatio = Math.Min(GetSubcategoryDiversity(subcategoryDiversity, category) / diversityTarget, 1m);
        var diversityScore = diversityRatio * 15m;
        var richnessRatio = (densityRatio * 0.45m) + (diversityRatio * 0.55m);
        var scoreCap = 58m + (42m * richnessRatio);

        return Math.Round(Math.Min(scoreCap, nearestScore + densityScore + diversityScore), 2);
    }

    private static decimal CalculateTransportScore(
        InfrastructureDetailsDto details,
        IReadOnlyDictionary<string, int> subcategoryDiversity)
    {
        var walkMinutes = details.SearchFeatures.NearestStopGroupWalkMinutes;
        var distanceMeters = details.SearchFeatures.NearestStopGroupDistanceM;
        var stopGroupsCountPrimary = details.SearchFeatures.StopGroupsCountPrimary;
        var stopPlatformsCountPrimary = details.SearchFeatures.StopPlatformsCountPrimary;

        decimal nearestScore;
        if (walkMinutes.HasValue)
        {
            nearestScore = Math.Clamp((12m - walkMinutes.Value) / 12m, 0m, 1m) * 45m;
        }
        else if (distanceMeters.HasValue)
        {
            nearestScore = Math.Clamp((800m - distanceMeters.Value) / 800m, 0m, 1m) * 45m;
        }
        else
        {
            nearestScore = 0m;
        }

        var groupsRatio = Math.Min(stopGroupsCountPrimary / 4m, 1m);
        var groupsScore = groupsRatio * 30m;
        var platformRatio = Math.Min(stopPlatformsCountPrimary / 8m, 1m);
        var platformScore = platformRatio * 10m;
        var diversityRatio = Math.Min(GetSubcategoryDiversity(subcategoryDiversity, "transport") / 4m, 1m);
        var diversityScore = diversityRatio * 15m;
        var richnessRatio = (groupsRatio * 0.4m) + (platformRatio * 0.2m) + (diversityRatio * 0.4m);
        var scoreCap = 60m + (40m * richnessRatio);

        return Math.Round(Math.Min(scoreCap, nearestScore + groupsScore + platformScore + diversityScore), 2);
    }

    private static decimal CalculateLeisureScore(
        InfrastructureDetailsDto details,
        IReadOnlyDictionary<string, LocationPoiSnapshotItem[]> grouped,
        IReadOnlyDictionary<string, int> subcategoryDiversity)
    {
        var parkAccess = CalculateAccessScore(details.SearchFeatures.NearestParkDistanceM, details.SearchFeatures.ParksCountPrimary, 1500m, 3m);
        var fitnessAccess = CalculateAccessScore(details.SearchFeatures.NearestFitnessDistanceM, details.SearchFeatures.FitnessCountPrimary, 2000m, 3m);

        var cultureDistances = new[]
        {
            details.SearchFeatures.NearestMuseumDistanceM,
            details.SearchFeatures.NearestTheaterDistanceM,
            details.SearchFeatures.NearestCinemaDistanceM
        }
        .Where(static value => value.HasValue)
        .Select(static value => value!.Value)
        .OrderBy(static value => value)
        .ToArray();

        var culturePrimaryCount =
            details.SearchFeatures.MuseumsCountPrimary +
            details.SearchFeatures.TheatersCountPrimary +
            details.SearchFeatures.CinemasCountPrimary;

        var cultureAccess = cultureDistances.Length == 0
            ? 0m
            : CalculateAccessScore(cultureDistances[0], culturePrimaryCount, 3000m, 3m);

        var leisurePrimaryCount = grouped
            .Where(static pair => pair.Key.Equals("leisure", StringComparison.OrdinalIgnoreCase))
            .SelectMany(static pair => pair.Value)
            .Count();
        var densityRatio = Math.Min(leisurePrimaryCount / 8m, 1m);
        var densityScore = densityRatio * 15m;
        var diversityRatio = Math.Min(GetSubcategoryDiversity(subcategoryDiversity, "leisure") / 6m, 1m);
        var diversityScore = diversityRatio * 10m;
        var pillarCoverage =
            (parkAccess > 0m ? 1m : 0m) +
            (fitnessAccess > 0m ? 1m : 0m) +
            (cultureAccess > 0m ? 1m : 0m);
        var pillarRatio = pillarCoverage / 3m;

        var rawScore =
            parkAccess * 0.35m +
            fitnessAccess * 0.20m +
            cultureAccess * 0.20m +
            densityScore +
            diversityScore;
        var richnessRatio =
            pillarRatio * 0.45m +
            diversityRatio * 0.35m +
            densityRatio * 0.20m;
        var scoreCap = 55m + (45m * richnessRatio);

        return Math.Round(Math.Min(scoreCap, rawScore), 2);
    }

    private static decimal GetSubcategoryDiversity(IReadOnlyDictionary<string, int> diversity, string category)
        => diversity.TryGetValue(category, out var value) ? value : 0m;

    private static decimal CalculateAccessScore(decimal? nearestDistanceMeters, int countPrimary, decimal radiusMeters, decimal targetCount)
    {
        if (!nearestDistanceMeters.HasValue && countPrimary <= 0)
            return 0m;

        var proximityScore = nearestDistanceMeters.HasValue
            ? Math.Clamp((radiusMeters - nearestDistanceMeters.Value) / radiusMeters, 0m, 1m) * 75m
            : 0m;
        var densityScore = Math.Min(countPrimary / targetCount, 1m) * 25m;
        return Math.Min(100m, proximityScore + densityScore);
    }

    private static decimal CalculateTotalScore(
        decimal education,
        decimal healthcare,
        decimal transport,
        decimal commerce,
        decimal leisure)
    {
        var weightedSum =
            education * CategoryWeights["education"] +
            healthcare * CategoryWeights["healthcare"] +
            transport * CategoryWeights["transport"] +
            commerce * CategoryWeights["commerce"] +
            leisure * CategoryWeights["leisure"];

        var totalWeight = CategoryWeights.Values.Sum();
        return Math.Round(weightedSum / totalWeight, 2);
    }

    private static bool IsInsidePrimaryRadius(LocationPoiSnapshotItem item)
    {
        var subcategoryCode = item.Poi?.SubcategoryCode;
        if (string.IsNullOrWhiteSpace(subcategoryCode))
            return item.DistanceMeters <= 1500m;

        var definition = InfrastructureMetadata.TryGetSubcategory(subcategoryCode);
        return definition is null || item.DistanceMeters <= definition.PrimaryRadiusMeters;
    }

    private static void AddIfValue(IDictionary<string, decimal> target, string key, decimal? value)
    {
        if (value.HasValue)
            target[key] = value.Value;
    }
}
