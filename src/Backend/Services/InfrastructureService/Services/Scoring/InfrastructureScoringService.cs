using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace InfrastructureService.Services.Scoring;

public sealed class InfrastructureScoringService(
    IOptions<InfrastructureScoringOptions> options,
    IInfrastructureScoringInputBuilder scoringInputBuilder,
    IEnumerable<IInfrastructureCategoryScoringPolicy> categoryPolicies) : IInfrastructureScoringService
{
    private readonly IReadOnlyDictionary<string, IInfrastructureCategoryScoringPolicy> _categoryPolicies = categoryPolicies
        .ToDictionary(static policy => policy.CategoryCode, StringComparer.OrdinalIgnoreCase);

    public InfrastructureScore Calculate(Location location, LocationPoiSnapshot snapshot, InfrastructureDetailsDto details)
    {
        var profile = options.Value.GetActiveProfile();
        var scoringInput = scoringInputBuilder.Build(snapshot, details);

        var education = ScoreCategory(location, "education", scoringInput, details, profile);
        var healthcare = ScoreCategory(location, "healthcare", scoringInput, details, profile);
        var transport = ScoreCategory(location, "transport", scoringInput, details, profile);
        var commerce = ScoreCategory(location, "commerce", scoringInput, details, profile);
        var leisure = ScoreCategory(location, "leisure", scoringInput, details, profile);
        var total = CalculateTotalScore(profile.TotalWeights, education.Score, healthcare.Score, transport.Score, commerce.Score, leisure.Score);

        var updatedDetails = new InfrastructureDetailsDto
        {
            NearbyCategories = details.NearbyCategories,
            TransportDetails = details.TransportDetails,
            SearchFeatures = details.SearchFeatures,
            CategoryScoreBreakdowns = new Dictionary<string, CategoryScoreBreakdownDto>(StringComparer.OrdinalIgnoreCase)
            {
                ["education"] = education,
                ["healthcare"] = healthcare,
                ["transport"] = transport,
                ["commerce"] = commerce,
                ["leisure"] = leisure
            },
            FilteredAddressLikePoiCount = details.FilteredAddressLikePoiCount,
            ExcludedPoiReasons = details.ExcludedPoiReasons,
            PrimaryRadiusConfig = details.PrimaryRadiusConfig,
            FallbackRadiusConfig = details.FallbackRadiusConfig,
            Highlights = details.Highlights,
            MissingPrimaryCategories = details.MissingPrimaryCategories,
            ClassificationVersions = details.ClassificationVersions,
            ScoringProfile = options.Value.ActiveProfile
        };

        var searchFeatures = BuildSearchFeaturesDictionary(scoringInput, updatedDetails, education, healthcare, transport, commerce, leisure);

        return new InfrastructureScore
        {
            Id = Guid.NewGuid(),
            LocationId = location.Id,
            SnapshotId = snapshot.Id,
            TotalScore = total,
            EducationScore = education.Score,
            HealthcareScore = healthcare.Score,
            TransportScore = transport.Score,
            CommerceScore = commerce.Score,
            LeisureScore = leisure.Score,
            PoiCount = snapshot.Items.Count,
            CalculatedAtUtc = DateTime.UtcNow,
            SearchFeaturesJson = JsonSerializer.Serialize(searchFeatures),
            DetailedSummaryJson = JsonSerializer.Serialize(updatedDetails)
        };
    }

    private CategoryScoreBreakdownDto ScoreCategory(
        Location location,
        string categoryCode,
        ScoringEntities.InfrastructureScoringInput scoringInput,
        InfrastructureDetailsDto details,
        InfrastructureScoringProfileOptions profile)
    {
        if (!_categoryPolicies.TryGetValue(categoryCode, out var policy))
        {
            return new CategoryScoreBreakdownDto();
        }

        return policy.Score(location, scoringInput.GetEntities(categoryCode), details, profile);
    }

    private Dictionary<string, decimal> BuildSearchFeaturesDictionary(
        ScoringEntities.InfrastructureScoringInput scoringInput,
        InfrastructureDetailsDto details,
        CategoryScoreBreakdownDto education,
        CategoryScoreBreakdownDto healthcare,
        CategoryScoreBreakdownDto transport,
        CategoryScoreBreakdownDto commerce,
        CategoryScoreBreakdownDto leisure)
    {
        var searchFeatures = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["education_score"] = education.Score,
            ["healthcare_score"] = healthcare.Score,
            ["transport_score"] = transport.Score,
            ["commerce_score"] = commerce.Score,
            ["leisure_score"] = leisure.Score,
            ["education_diversity_score"] = education.DiversityScore,
            ["healthcare_diversity_score"] = healthcare.DiversityScore,
            ["transport_diversity_score"] = transport.DiversityScore,
            ["commerce_diversity_score"] = commerce.DiversityScore,
            ["leisure_diversity_score"] = leisure.DiversityScore,
            ["education_density_score"] = education.DensityScore,
            ["healthcare_density_score"] = healthcare.DensityScore,
            ["transport_density_score"] = transport.DensityScore,
            ["commerce_density_score"] = commerce.DensityScore,
            ["leisure_density_score"] = leisure.DensityScore,
            ["education_proximity_score"] = education.ProximityScore,
            ["healthcare_proximity_score"] = healthcare.ProximityScore,
            ["transport_proximity_score"] = transport.ProximityScore,
            ["commerce_proximity_score"] = commerce.ProximityScore,
            ["leisure_proximity_score"] = leisure.ProximityScore,
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
            ["filtered_address_like_poi_count"] = details.FilteredAddressLikePoiCount
        };

        foreach (var category in new[] { "education", "healthcare", "transport", "commerce", "leisure" })
        {
            var entities = scoringInput.GetEntities(category)
                .OrderBy(static entity => entity.DistanceMeters)
                .ToArray();
            searchFeatures[$"{category}_count"] = entities.Length;
            if (entities.Length > 0)
            {
                searchFeatures[$"{category}_nearest_m"] = entities[0].DistanceMeters;
            }
        }

        AddGroupedCounts(searchFeatures, "education", education);
        AddGroupedCounts(searchFeatures, "healthcare", healthcare);
        AddGroupedCounts(searchFeatures, "transport", transport);
        AddGroupedCounts(searchFeatures, "commerce", commerce);
        AddGroupedCounts(searchFeatures, "leisure", leisure);

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

        return searchFeatures;
    }

    private static void AddGroupedCounts(IDictionary<string, decimal> target, string categoryCode, CategoryScoreBreakdownDto breakdown)
    {
        foreach (var (subcategory, count) in breakdown.GroupedCounts)
        {
            target[$"{categoryCode}_{subcategory}_grouped_count"] = count;
        }
    }

    private static decimal CalculateTotalScore(
        CategoryWeightOptions weights,
        decimal education,
        decimal healthcare,
        decimal transport,
        decimal commerce,
        decimal leisure)
    {
        var weightedSum =
            education * weights.Education +
            healthcare * weights.Healthcare +
            transport * weights.Transport +
            commerce * weights.Commerce +
            leisure * weights.Leisure;

        var totalWeight = weights.Education + weights.Healthcare + weights.Transport + weights.Commerce + weights.Leisure;
        return Math.Round(weightedSum / totalWeight, 2, MidpointRounding.AwayFromZero);
    }

    private static void AddIfValue(IDictionary<string, decimal> target, string key, decimal? value)
    {
        if (value.HasValue)
        {
            target[key] = value.Value;
        }
    }
}
