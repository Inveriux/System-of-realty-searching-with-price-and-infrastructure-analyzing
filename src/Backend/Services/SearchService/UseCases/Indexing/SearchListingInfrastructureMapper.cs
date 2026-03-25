using System.Text.Json;
using SearchService.Models;
using SearchService.Services.InternalClients.Contracts;

namespace SearchService.UseCases.Indexing;

internal static class SearchListingInfrastructureMapper
{
    public static void Apply(SearchListing listing, InfrastructureScorePayloadDto? infrastructureScore)
    {
        var features = infrastructureScore?.SearchFeatures ?? [];
        var detailed = infrastructureScore?.Details.SearchFeatures;

        listing.InfrastructureScore = infrastructureScore?.TotalScore;
        listing.EducationScore = infrastructureScore?.EducationScore;
        listing.HealthcareScore = infrastructureScore?.HealthcareScore;
        listing.TransportScore = infrastructureScore?.TransportScore;
        listing.CommerceScore = infrastructureScore?.CommerceScore;
        listing.LeisureScore = infrastructureScore?.LeisureScore;
        listing.EducationCount = GetInt(features, "education_count");
        listing.HealthcareCount = GetInt(features, "healthcare_count");
        listing.TransportCount = GetInt(features, "transport_count");
        listing.CommerceCount = GetInt(features, "commerce_count");
        listing.LeisureCount = GetInt(features, "leisure_count");
        listing.EducationNearestMeters = GetDecimal(features, "education_nearest_m");
        listing.HealthcareNearestMeters = GetDecimal(features, "healthcare_nearest_m");
        listing.TransportNearestMeters = GetDecimal(features, "transport_nearest_m");
        listing.CommerceNearestMeters = GetDecimal(features, "commerce_nearest_m");
        listing.LeisureNearestMeters = GetDecimal(features, "leisure_nearest_m");
        listing.PharmacyCountPrimary = detailed?.PharmacyCountPrimary;
        listing.StoreCountPrimary = detailed?.StoreCountPrimary;
        listing.StopsCountPrimary = detailed?.StopsCountPrimary;
        listing.StopGroupsCountPrimary = detailed?.StopGroupsCountPrimary;
        listing.StopPlatformsCountPrimary = detailed?.StopPlatformsCountPrimary;
        listing.ParkingCountPrimary = detailed?.ParkingCountPrimary;
        listing.SchoolCountPrimary = detailed?.SchoolCountPrimary;
        listing.KindergartenCountPrimary = detailed?.KindergartenCountPrimary;
        listing.ClinicCountPrimary = detailed?.ClinicCountPrimary;
        listing.HospitalCountPrimary = detailed?.HospitalCountPrimary;
        listing.NearestPharmacyWalkMinutes = detailed?.NearestPharmacyWalkMinutes;
        listing.NearestStoreWalkMinutes = detailed?.NearestStoreWalkMinutes;
        listing.NearestStopWalkMinutes = detailed?.NearestStopWalkMinutes;
        listing.NearestStopGroupWalkMinutes = detailed?.NearestStopGroupWalkMinutes;
        listing.NearestStopGroupDriveMinutes = detailed?.NearestStopGroupDriveMinutes;
        listing.NearestStopGroupDriveMinutesEstimated = detailed?.NearestStopGroupDriveMinutesEstimated ?? false;
        listing.NearestStopGroupName = detailed?.NearestStopGroupName;
        listing.NearestStopGroupPlatformCount = detailed?.NearestStopGroupPlatformCount;
        listing.NearestParkingWalkMinutes = detailed?.NearestParkingWalkMinutes;
        listing.NearestParkingDriveMinutes = detailed?.NearestParkingDriveMinutes;
        listing.NearestParkingDriveMinutesEstimated = detailed?.NearestParkingDriveMinutesEstimated ?? false;
        listing.NearestSchoolWalkMinutes = detailed?.NearestSchoolWalkMinutes;
        listing.NearestKindergartenWalkMinutes = detailed?.NearestKindergartenWalkMinutes;
        listing.NearestClinicDriveMinutes = detailed?.NearestClinicDriveMinutes;
        listing.NearestClinicDriveMinutesEstimated = detailed?.NearestClinicDriveMinutesEstimated ?? false;
        listing.NearestHospitalDriveMinutes = detailed?.NearestHospitalDriveMinutes;
        listing.NearestHospitalDriveMinutesEstimated = detailed?.NearestHospitalDriveMinutesEstimated ?? false;
        listing.ParksCountPrimary = detailed?.ParksCountPrimary;
        listing.NearestParkWalkMinutes = detailed?.NearestParkWalkMinutes;
        listing.FitnessCountPrimary = detailed?.FitnessCountPrimary;
        listing.NearestFitnessWalkMinutes = detailed?.NearestFitnessWalkMinutes;
        listing.NearestFitnessDriveMinutes = detailed?.NearestFitnessDriveMinutes;
        listing.MuseumsCountPrimary = detailed?.MuseumsCountPrimary;
        listing.NearestMuseumDriveMinutes = detailed?.NearestMuseumDriveMinutes;
        listing.TheatersCountPrimary = detailed?.TheatersCountPrimary;
        listing.NearestTheaterDriveMinutes = detailed?.NearestTheaterDriveMinutes;
        listing.CinemasCountPrimary = detailed?.CinemasCountPrimary;
        listing.NearestCinemaDriveMinutes = detailed?.NearestCinemaDriveMinutes;
        listing.NearestMetroWalkMinutes = detailed?.NearestMetroWalkMinutes;
        listing.NearestMetroName = detailed?.NearestMetroName;
        listing.MetroFoundInPrimaryRadius = detailed?.MetroFoundInPrimaryRadius ?? false;
        listing.SearchFeaturesJson = JsonSerializer.Serialize(features);
    }

    private static int? GetInt(IReadOnlyDictionary<string, decimal> features, string key)
        => features.TryGetValue(key, out var value) ? (int)value : null;

    private static decimal? GetDecimal(IReadOnlyDictionary<string, decimal> features, string key)
        => features.TryGetValue(key, out var value) ? value : null;
}
