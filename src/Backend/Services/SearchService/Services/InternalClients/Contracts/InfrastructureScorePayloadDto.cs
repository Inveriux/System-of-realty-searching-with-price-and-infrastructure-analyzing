namespace SearchService.Services.InternalClients.Contracts;

public sealed class InfrastructureScorePayloadDto
{
    public Guid LocationId { get; init; }
    public decimal TotalScore { get; init; }
    public decimal EducationScore { get; init; }
    public decimal HealthcareScore { get; init; }
    public decimal TransportScore { get; init; }
    public decimal CommerceScore { get; init; }
    public decimal LeisureScore { get; init; }
    public int PoiCount { get; init; }
    public DateTime CalculatedAtUtc { get; init; }
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];
    public InfrastructureDetailsPayloadDto Details { get; init; } = new();
}

public sealed class InfrastructureDetailsPayloadDto
{
    public InfrastructureSearchFeaturesPayloadDto SearchFeatures { get; init; } = new();
}

public sealed class InfrastructureSearchFeaturesPayloadDto
{
    public int StoreCountPrimary { get; init; }
    public int PharmacyCountPrimary { get; init; }
    public int StopsCountPrimary { get; init; }
    public int StopGroupsCountPrimary { get; init; }
    public int StopPlatformsCountPrimary { get; init; }
    public int ParkingCountPrimary { get; init; }
    public int SchoolCountPrimary { get; init; }
    public int KindergartenCountPrimary { get; init; }
    public int ClinicCountPrimary { get; init; }
    public int HospitalCountPrimary { get; init; }
    public decimal? NearestPharmacyWalkMinutes { get; init; }
    public decimal? NearestPharmacyDriveMinutes { get; init; }
    public bool NearestPharmacyDriveMinutesEstimated { get; init; }
    public decimal? NearestStoreWalkMinutes { get; init; }
    public decimal? NearestStoreDriveMinutes { get; init; }
    public bool NearestStoreDriveMinutesEstimated { get; init; }
    public decimal? NearestStopWalkMinutes { get; init; }
    public decimal? NearestStopDriveMinutes { get; init; }
    public bool NearestStopDriveMinutesEstimated { get; init; }
    public decimal? NearestStopGroupWalkMinutes { get; init; }
    public decimal? NearestStopGroupDriveMinutes { get; init; }
    public bool NearestStopGroupDriveMinutesEstimated { get; init; }
    public string? NearestStopGroupName { get; init; }
    public int? NearestStopGroupPlatformCount { get; init; }
    public decimal? NearestParkingWalkMinutes { get; init; }
    public decimal? NearestParkingDriveMinutes { get; init; }
    public bool NearestParkingDriveMinutesEstimated { get; init; }
    public decimal? NearestSchoolWalkMinutes { get; init; }
    public decimal? NearestSchoolDriveMinutes { get; init; }
    public bool NearestSchoolDriveMinutesEstimated { get; init; }
    public decimal? NearestKindergartenWalkMinutes { get; init; }
    public decimal? NearestKindergartenDriveMinutes { get; init; }
    public bool NearestKindergartenDriveMinutesEstimated { get; init; }
    public decimal? NearestClinicDriveMinutes { get; init; }
    public bool NearestClinicDriveMinutesEstimated { get; init; }
    public decimal? NearestHospitalDriveMinutes { get; init; }
    public bool NearestHospitalDriveMinutesEstimated { get; init; }
    public int ParksCountPrimary { get; init; }
    public decimal? NearestParkWalkMinutes { get; init; }
    public decimal? NearestParkDriveMinutes { get; init; }
    public decimal? NearestParkAnyWalkMinutes { get; init; }
    public decimal? NearestParkAnyDriveMinutes { get; init; }
    public decimal? NearestParkDistanceM { get; init; }
    public decimal? NearestParkAnyDistanceM { get; init; }
    public int? NearestParkFoundRadiusM { get; init; }
    public int FitnessCountPrimary { get; init; }
    public decimal? NearestFitnessWalkMinutes { get; init; }
    public decimal? NearestFitnessDriveMinutes { get; init; }
    public decimal? NearestFitnessAnyWalkMinutes { get; init; }
    public decimal? NearestFitnessAnyDriveMinutes { get; init; }
    public decimal? NearestFitnessDistanceM { get; init; }
    public decimal? NearestFitnessAnyDistanceM { get; init; }
    public int? NearestFitnessFoundRadiusM { get; init; }
    public int MuseumsCountPrimary { get; init; }
    public decimal? NearestMuseumWalkMinutes { get; init; }
    public decimal? NearestMuseumDriveMinutes { get; init; }
    public decimal? NearestMuseumAnyWalkMinutes { get; init; }
    public decimal? NearestMuseumAnyDriveMinutes { get; init; }
    public decimal? NearestMuseumDistanceM { get; init; }
    public decimal? NearestMuseumAnyDistanceM { get; init; }
    public int? NearestMuseumFoundRadiusM { get; init; }
    public int TheatersCountPrimary { get; init; }
    public decimal? NearestTheaterWalkMinutes { get; init; }
    public decimal? NearestTheaterDriveMinutes { get; init; }
    public decimal? NearestTheaterAnyWalkMinutes { get; init; }
    public decimal? NearestTheaterAnyDriveMinutes { get; init; }
    public decimal? NearestTheaterDistanceM { get; init; }
    public decimal? NearestTheaterAnyDistanceM { get; init; }
    public int? NearestTheaterFoundRadiusM { get; init; }
    public int CinemasCountPrimary { get; init; }
    public decimal? NearestCinemaWalkMinutes { get; init; }
    public decimal? NearestCinemaDriveMinutes { get; init; }
    public decimal? NearestCinemaAnyWalkMinutes { get; init; }
    public decimal? NearestCinemaAnyDriveMinutes { get; init; }
    public decimal? NearestCinemaDistanceM { get; init; }
    public decimal? NearestCinemaAnyDistanceM { get; init; }
    public int? NearestCinemaFoundRadiusM { get; init; }
    public int CafesCountPrimary { get; init; }
    public int RestaurantsCountPrimary { get; init; }
    public decimal? NearestMetroWalkMinutes { get; init; }
    public string? NearestMetroName { get; init; }
    public bool MetroFoundInPrimaryRadius { get; init; }
}
