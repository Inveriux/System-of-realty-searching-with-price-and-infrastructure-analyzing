namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureSearchFeaturesDto
{
    public int StoreCountPrimary { get; init; }
    public decimal? NearestStoreDistanceM { get; init; }
    public decimal? NearestStoreWalkMinutes { get; init; }
    public decimal? NearestStoreDriveMinutes { get; init; }
    public bool NearestStoreDriveMinutesEstimated { get; init; }
    public decimal? NearestStoreAnyDistanceM { get; init; }
    public decimal? NearestStoreAnyWalkMinutes { get; init; }
    public decimal? NearestStoreAnyDriveMinutes { get; init; }
    public bool NearestStoreAnyDriveMinutesEstimated { get; init; }
    public int? NearestStoreFoundRadiusM { get; init; }

    public int PharmacyCountPrimary { get; init; }
    public decimal? NearestPharmacyDistanceM { get; init; }
    public decimal? NearestPharmacyWalkMinutes { get; init; }
    public decimal? NearestPharmacyDriveMinutes { get; init; }
    public bool NearestPharmacyDriveMinutesEstimated { get; init; }
    public decimal? NearestPharmacyAnyDistanceM { get; init; }
    public decimal? NearestPharmacyAnyWalkMinutes { get; init; }
    public decimal? NearestPharmacyAnyDriveMinutes { get; init; }
    public bool NearestPharmacyAnyDriveMinutesEstimated { get; init; }
    public int? NearestPharmacyFoundRadiusM { get; init; }

    public int StopsCountPrimary { get; init; }
    public decimal? NearestStopDistanceM { get; init; }
    public decimal? NearestStopWalkMinutes { get; init; }
    public decimal? NearestStopDriveMinutes { get; init; }
    public bool NearestStopDriveMinutesEstimated { get; init; }
    public decimal? NearestStopAnyDistanceM { get; init; }
    public decimal? NearestStopAnyWalkMinutes { get; init; }
    public decimal? NearestStopAnyDriveMinutes { get; init; }
    public bool NearestStopAnyDriveMinutesEstimated { get; init; }
    public int? NearestStopFoundRadiusM { get; init; }
    public int StopGroupsCountPrimary { get; init; }
    public int StopPlatformsCountPrimary { get; init; }
    public decimal? NearestStopGroupDistanceM { get; init; }
    public decimal? NearestStopGroupWalkMinutes { get; init; }
    public decimal? NearestStopGroupDriveMinutes { get; init; }
    public bool NearestStopGroupDriveMinutesEstimated { get; init; }
    public string? NearestStopGroupName { get; init; }
    public int? NearestStopGroupPlatformCount { get; init; }
    public decimal? NearestStopGroupAnyDistanceM { get; init; }
    public decimal? NearestStopGroupAnyWalkMinutes { get; init; }
    public decimal? NearestStopGroupAnyDriveMinutes { get; init; }
    public bool NearestStopGroupAnyDriveMinutesEstimated { get; init; }
    public int? NearestStopGroupFoundRadiusM { get; init; }

    public int ParkingCountPrimary { get; init; }
    public decimal? NearestParkingDistanceM { get; init; }
    public decimal? NearestParkingWalkMinutes { get; init; }
    public decimal? NearestParkingDriveMinutes { get; init; }
    public bool NearestParkingDriveMinutesEstimated { get; init; }
    public decimal? NearestParkingAnyDistanceM { get; init; }
    public decimal? NearestParkingAnyWalkMinutes { get; init; }
    public decimal? NearestParkingAnyDriveMinutes { get; init; }
    public bool NearestParkingAnyDriveMinutesEstimated { get; init; }
    public int? NearestParkingFoundRadiusM { get; init; }

    public int SchoolCountPrimary { get; init; }
    public decimal? NearestSchoolDistanceM { get; init; }
    public decimal? NearestSchoolWalkMinutes { get; init; }
    public decimal? NearestSchoolDriveMinutes { get; init; }
    public bool NearestSchoolDriveMinutesEstimated { get; init; }
    public decimal? NearestSchoolAnyDistanceM { get; init; }
    public decimal? NearestSchoolAnyWalkMinutes { get; init; }
    public decimal? NearestSchoolAnyDriveMinutes { get; init; }
    public bool NearestSchoolAnyDriveMinutesEstimated { get; init; }
    public int? NearestSchoolFoundRadiusM { get; init; }

    public int KindergartenCountPrimary { get; init; }
    public decimal? NearestKindergartenDistanceM { get; init; }
    public decimal? NearestKindergartenWalkMinutes { get; init; }
    public decimal? NearestKindergartenDriveMinutes { get; init; }
    public bool NearestKindergartenDriveMinutesEstimated { get; init; }
    public decimal? NearestKindergartenAnyDistanceM { get; init; }
    public decimal? NearestKindergartenAnyWalkMinutes { get; init; }
    public decimal? NearestKindergartenAnyDriveMinutes { get; init; }
    public bool NearestKindergartenAnyDriveMinutesEstimated { get; init; }
    public int? NearestKindergartenFoundRadiusM { get; init; }

    public int ClinicCountPrimary { get; init; }
    public decimal? NearestClinicDistanceM { get; init; }
    public decimal? NearestClinicWalkMinutes { get; init; }
    public decimal? NearestClinicDriveMinutes { get; init; }
    public bool NearestClinicDriveMinutesEstimated { get; init; }
    public decimal? NearestClinicAnyDistanceM { get; init; }
    public decimal? NearestClinicAnyWalkMinutes { get; init; }
    public decimal? NearestClinicAnyDriveMinutes { get; init; }
    public bool NearestClinicAnyDriveMinutesEstimated { get; init; }
    public int? NearestClinicFoundRadiusM { get; init; }

    public int HospitalCountPrimary { get; init; }
    public decimal? NearestHospitalDistanceM { get; init; }
    public decimal? NearestHospitalWalkMinutes { get; init; }
    public decimal? NearestHospitalDriveMinutes { get; init; }
    public bool NearestHospitalDriveMinutesEstimated { get; init; }
    public decimal? NearestHospitalAnyDistanceM { get; init; }
    public decimal? NearestHospitalAnyWalkMinutes { get; init; }
    public decimal? NearestHospitalAnyDriveMinutes { get; init; }
    public bool NearestHospitalAnyDriveMinutesEstimated { get; init; }
    public int? NearestHospitalFoundRadiusM { get; init; }

    public int LeisureCountPrimary { get; init; }
    public decimal? NearestLeisureDistanceM { get; init; }
    public decimal? NearestLeisureWalkMinutes { get; init; }
    public decimal? NearestLeisureDriveMinutes { get; init; }
    public bool NearestLeisureDriveMinutesEstimated { get; init; }
    public decimal? NearestLeisureAnyDistanceM { get; init; }
    public decimal? NearestLeisureAnyWalkMinutes { get; init; }
    public decimal? NearestLeisureAnyDriveMinutes { get; init; }
    public bool NearestLeisureAnyDriveMinutesEstimated { get; init; }
    public int? NearestLeisureFoundRadiusM { get; init; }

    public int ParksCountPrimary { get; init; }
    public decimal? NearestParkDistanceM { get; init; }
    public decimal? NearestParkWalkMinutes { get; init; }
    public decimal? NearestParkDriveMinutes { get; init; }
    public bool NearestParkDriveMinutesEstimated { get; init; }
    public decimal? NearestParkAnyDistanceM { get; init; }
    public decimal? NearestParkAnyWalkMinutes { get; init; }
    public decimal? NearestParkAnyDriveMinutes { get; init; }
    public bool NearestParkAnyDriveMinutesEstimated { get; init; }
    public int? NearestParkFoundRadiusM { get; init; }

    public int FitnessCountPrimary { get; init; }
    public decimal? NearestFitnessDistanceM { get; init; }
    public decimal? NearestFitnessWalkMinutes { get; init; }
    public decimal? NearestFitnessDriveMinutes { get; init; }
    public bool NearestFitnessDriveMinutesEstimated { get; init; }
    public decimal? NearestFitnessAnyDistanceM { get; init; }
    public decimal? NearestFitnessAnyWalkMinutes { get; init; }
    public decimal? NearestFitnessAnyDriveMinutes { get; init; }
    public bool NearestFitnessAnyDriveMinutesEstimated { get; init; }
    public int? NearestFitnessFoundRadiusM { get; init; }

    public int MuseumsCountPrimary { get; init; }
    public decimal? NearestMuseumDistanceM { get; init; }
    public decimal? NearestMuseumWalkMinutes { get; init; }
    public decimal? NearestMuseumDriveMinutes { get; init; }
    public bool NearestMuseumDriveMinutesEstimated { get; init; }
    public decimal? NearestMuseumAnyDistanceM { get; init; }
    public decimal? NearestMuseumAnyWalkMinutes { get; init; }
    public decimal? NearestMuseumAnyDriveMinutes { get; init; }
    public bool NearestMuseumAnyDriveMinutesEstimated { get; init; }
    public int? NearestMuseumFoundRadiusM { get; init; }

    public int TheatersCountPrimary { get; init; }
    public decimal? NearestTheaterDistanceM { get; init; }
    public decimal? NearestTheaterWalkMinutes { get; init; }
    public decimal? NearestTheaterDriveMinutes { get; init; }
    public bool NearestTheaterDriveMinutesEstimated { get; init; }
    public decimal? NearestTheaterAnyDistanceM { get; init; }
    public decimal? NearestTheaterAnyWalkMinutes { get; init; }
    public decimal? NearestTheaterAnyDriveMinutes { get; init; }
    public bool NearestTheaterAnyDriveMinutesEstimated { get; init; }
    public int? NearestTheaterFoundRadiusM { get; init; }

    public int CinemasCountPrimary { get; init; }
    public decimal? NearestCinemaDistanceM { get; init; }
    public decimal? NearestCinemaWalkMinutes { get; init; }
    public decimal? NearestCinemaDriveMinutes { get; init; }
    public bool NearestCinemaDriveMinutesEstimated { get; init; }
    public decimal? NearestCinemaAnyDistanceM { get; init; }
    public decimal? NearestCinemaAnyWalkMinutes { get; init; }
    public decimal? NearestCinemaAnyDriveMinutes { get; init; }
    public bool NearestCinemaAnyDriveMinutesEstimated { get; init; }
    public int? NearestCinemaFoundRadiusM { get; init; }

    public int CafesCountPrimary { get; init; }
    public int RestaurantsCountPrimary { get; init; }

    public decimal? NearestMetroDistanceM { get; init; }
    public decimal? NearestMetroWalkMinutes { get; init; }
    public string? NearestMetroName { get; init; }
    public bool MetroFoundInPrimaryRadius { get; init; }
}
