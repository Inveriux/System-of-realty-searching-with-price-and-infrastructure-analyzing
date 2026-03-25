using System.Text.Json;

namespace ListingPageBff.Infrastructure.Clients.Contracts;

public sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
}

public sealed class DownstreamDetailedListingDto
{
    public Guid Id { get; init; }
    public Guid AgentId { get; init; }
    public Guid? LocationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AddressText { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? PriceAmount { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
    public string? PricePeriod { get; init; }
    public decimal AreaTotal { get; init; }
    public int? BuildYear { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public string? BuildingType { get; init; }
    public string ListingType { get; init; } = string.Empty;
    public string? MainMedia { get; init; }
    public IReadOnlyCollection<DownstreamListingMediaDto> Media { get; init; } = [];
    public IReadOnlyCollection<DownstreamListingPriceHistoryDto> PriceHistory { get; init; } = [];
    public string? ListingDetailsType { get; init; }
    public JsonElement? ListingDetails { get; init; }
}

public sealed class DownstreamListingMediaDto
{
    public string Url { get; init; } = string.Empty;
}

public sealed class DownstreamListingPriceHistoryDto
{
    public DateTimeOffset ChangedAt { get; init; }
    public decimal? PriceAmount { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
}

public sealed class DownstreamInfrastructureSummaryDto
{
    public Guid LocationId { get; init; }
    public string AddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public DownstreamInfrastructureScoreDto Score { get; init; } = new();
    public string? Summary { get; init; }
    public IReadOnlyCollection<string> Highlights { get; init; } = [];
    public IReadOnlyCollection<DownstreamPoiDto> Pois { get; init; } = [];
    public IReadOnlyCollection<DownstreamInfrastructureNearbyCategoryDto> NearbyCategories { get; init; } = [];
    public DownstreamInfrastructureTransportDetailsDto TransportDetails { get; init; } = new();
    public DownstreamInfrastructureSearchFeaturesDto SearchFeatures { get; init; } = new();
    public Dictionary<string, int> PrimaryRadiusConfig { get; init; } = [];
    public Dictionary<string, int?> FallbackRadiusConfig { get; init; } = [];
    public IReadOnlyCollection<string> MissingPrimaryCategories { get; init; } = [];
    public Dictionary<string, string> ClassificationVersions { get; init; } = [];
}

public sealed class DownstreamRefreshInfrastructureRequestDto
{
    public Guid? LocationId { get; init; }
    public string? AddressText { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public int SearchRadiusMeters { get; init; } = 1500;
}

public sealed class DownstreamInfrastructureScoreDto
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
}

public sealed class DownstreamPoiDto
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AddressText { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? SubcategoryCode { get; init; }
    public string? Source { get; init; }
    public decimal DistanceMeters { get; init; }
}

public sealed class DownstreamInfrastructureNearbyCategoryDto
{
    public string CategoryCode { get; init; } = string.Empty;
    public string SubcategoryCode { get; init; } = string.Empty;
    public int CountInPrimaryRadius { get; init; }
    public decimal? NearestDistanceM { get; init; }
    public decimal? NearestWalkMinutes { get; init; }
    public decimal? NearestDriveMinutes { get; init; }
    public bool NearestDriveMinutesEstimated { get; init; }
    public decimal? NearestAnyDistanceM { get; init; }
    public decimal? NearestAnyWalkMinutes { get; init; }
    public decimal? NearestAnyDriveMinutes { get; init; }
    public bool NearestAnyDriveMinutesEstimated { get; init; }
    public int? FoundInRadiusM { get; init; }
    public bool FoundInPrimaryRadius { get; init; }
}

public sealed class DownstreamInfrastructureTransportDetailsDto
{
    public decimal? NearestStopDistanceM { get; init; }
    public decimal? NearestStopWalkMinutes { get; init; }
    public int StopsCountPrimary { get; init; }
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
    public IReadOnlyCollection<DownstreamInfrastructureTransportStopGroupDto> StopGroups { get; init; } = [];
    public decimal? NearestMetroDistanceM { get; init; }
    public decimal? NearestMetroWalkMinutes { get; init; }
    public string? NearestMetroName { get; init; }
    public bool MetroFoundInPrimaryRadius { get; init; }
    public decimal? NearestParkingDistanceM { get; init; }
    public decimal? NearestParkingWalkMinutes { get; init; }
    public decimal? NearestParkingDriveMinutes { get; init; }
    public bool NearestParkingDriveMinutesEstimated { get; init; }
    public int ParkingCountPrimary { get; init; }
}

public sealed class DownstreamInfrastructureTransportStopGroupDto
{
    public string GroupId { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public decimal CenterLatitude { get; init; }
    public decimal CenterLongitude { get; init; }
    public decimal MaxDistanceWithinGroup { get; init; }
    public int PointCount { get; init; }
    public int PlatformCount { get; init; }
    public IReadOnlyCollection<string> StopTypes { get; init; } = [];
    public IReadOnlyCollection<Guid> SourcePoiIds { get; init; } = [];
    public decimal NearestDistanceMeters { get; init; }
}

public sealed class DownstreamInfrastructureSearchFeaturesDto
{
    public int StoreCountPrimary { get; init; }
    public decimal? NearestStoreWalkMinutes { get; init; }
    public decimal? NearestStoreDriveMinutes { get; init; }
    public bool NearestStoreDriveMinutesEstimated { get; init; }
    public decimal? NearestStoreAnyWalkMinutes { get; init; }
    public decimal? NearestStoreAnyDriveMinutes { get; init; }
    public bool NearestStoreAnyDriveMinutesEstimated { get; init; }
    public int? NearestStoreFoundRadiusM { get; init; }
    public int PharmacyCountPrimary { get; init; }
    public decimal? NearestPharmacyWalkMinutes { get; init; }
    public decimal? NearestPharmacyDriveMinutes { get; init; }
    public bool NearestPharmacyDriveMinutesEstimated { get; init; }
    public decimal? NearestPharmacyAnyWalkMinutes { get; init; }
    public decimal? NearestPharmacyAnyDriveMinutes { get; init; }
    public bool NearestPharmacyAnyDriveMinutesEstimated { get; init; }
    public int? NearestPharmacyFoundRadiusM { get; init; }
    public int StopsCountPrimary { get; init; }
    public decimal? NearestStopWalkMinutes { get; init; }
    public decimal? NearestStopDriveMinutes { get; init; }
    public bool NearestStopDriveMinutesEstimated { get; init; }
    public decimal? NearestStopAnyWalkMinutes { get; init; }
    public decimal? NearestStopAnyDriveMinutes { get; init; }
    public bool NearestStopAnyDriveMinutesEstimated { get; init; }
    public int? NearestStopFoundRadiusM { get; init; }
    public int StopGroupsCountPrimary { get; init; }
    public int StopPlatformsCountPrimary { get; init; }
    public decimal? NearestStopGroupWalkMinutes { get; init; }
    public decimal? NearestStopGroupDriveMinutes { get; init; }
    public bool NearestStopGroupDriveMinutesEstimated { get; init; }
    public string? NearestStopGroupName { get; init; }
    public int? NearestStopGroupPlatformCount { get; init; }
    public decimal? NearestStopGroupAnyWalkMinutes { get; init; }
    public decimal? NearestStopGroupAnyDriveMinutes { get; init; }
    public bool NearestStopGroupAnyDriveMinutesEstimated { get; init; }
    public int? NearestStopGroupFoundRadiusM { get; init; }
    public int ParkingCountPrimary { get; init; }
    public decimal? NearestParkingWalkMinutes { get; init; }
    public decimal? NearestParkingDriveMinutes { get; init; }
    public bool NearestParkingDriveMinutesEstimated { get; init; }
    public decimal? NearestParkingAnyWalkMinutes { get; init; }
    public decimal? NearestParkingAnyDriveMinutes { get; init; }
    public bool NearestParkingAnyDriveMinutesEstimated { get; init; }
    public int? NearestParkingFoundRadiusM { get; init; }
    public int SchoolCountPrimary { get; init; }
    public decimal? NearestSchoolWalkMinutes { get; init; }
    public decimal? NearestSchoolDriveMinutes { get; init; }
    public bool NearestSchoolDriveMinutesEstimated { get; init; }
    public decimal? NearestSchoolAnyWalkMinutes { get; init; }
    public decimal? NearestSchoolAnyDriveMinutes { get; init; }
    public bool NearestSchoolAnyDriveMinutesEstimated { get; init; }
    public int? NearestSchoolFoundRadiusM { get; init; }
    public int KindergartenCountPrimary { get; init; }
    public decimal? NearestKindergartenWalkMinutes { get; init; }
    public decimal? NearestKindergartenDriveMinutes { get; init; }
    public bool NearestKindergartenDriveMinutesEstimated { get; init; }
    public decimal? NearestKindergartenAnyWalkMinutes { get; init; }
    public decimal? NearestKindergartenAnyDriveMinutes { get; init; }
    public bool NearestKindergartenAnyDriveMinutesEstimated { get; init; }
    public int? NearestKindergartenFoundRadiusM { get; init; }
    public int ClinicCountPrimary { get; init; }
    public decimal? NearestClinicWalkMinutes { get; init; }
    public decimal? NearestClinicDriveMinutes { get; init; }
    public bool NearestClinicDriveMinutesEstimated { get; init; }
    public decimal? NearestClinicAnyDriveMinutes { get; init; }
    public bool NearestClinicAnyDriveMinutesEstimated { get; init; }
    public int? NearestClinicFoundRadiusM { get; init; }
    public int HospitalCountPrimary { get; init; }
    public decimal? NearestHospitalDriveMinutes { get; init; }
    public bool NearestHospitalDriveMinutesEstimated { get; init; }
    public decimal? NearestHospitalAnyDriveMinutes { get; init; }
    public bool NearestHospitalAnyDriveMinutesEstimated { get; init; }
    public int? NearestHospitalFoundRadiusM { get; init; }
    public int LeisureCountPrimary { get; init; }
    public decimal? NearestLeisureWalkMinutes { get; init; }
    public decimal? NearestLeisureAnyWalkMinutes { get; init; }
    public int? NearestLeisureFoundRadiusM { get; init; }
    public int ParksCountPrimary { get; init; }
    public decimal? NearestParkWalkMinutes { get; init; }
    public decimal? NearestParkDriveMinutes { get; init; }
    public decimal? NearestParkAnyWalkMinutes { get; init; }
    public decimal? NearestParkAnyDriveMinutes { get; init; }
    public int? NearestParkFoundRadiusM { get; init; }
    public int FitnessCountPrimary { get; init; }
    public decimal? NearestFitnessWalkMinutes { get; init; }
    public decimal? NearestFitnessDriveMinutes { get; init; }
    public decimal? NearestFitnessAnyWalkMinutes { get; init; }
    public decimal? NearestFitnessAnyDriveMinutes { get; init; }
    public int? NearestFitnessFoundRadiusM { get; init; }
    public int MuseumsCountPrimary { get; init; }
    public decimal? NearestMuseumWalkMinutes { get; init; }
    public decimal? NearestMuseumDriveMinutes { get; init; }
    public decimal? NearestMuseumAnyWalkMinutes { get; init; }
    public decimal? NearestMuseumAnyDriveMinutes { get; init; }
    public int? NearestMuseumFoundRadiusM { get; init; }
    public int TheatersCountPrimary { get; init; }
    public decimal? NearestTheaterWalkMinutes { get; init; }
    public decimal? NearestTheaterDriveMinutes { get; init; }
    public decimal? NearestTheaterAnyWalkMinutes { get; init; }
    public decimal? NearestTheaterAnyDriveMinutes { get; init; }
    public int? NearestTheaterFoundRadiusM { get; init; }
    public int CinemasCountPrimary { get; init; }
    public decimal? NearestCinemaWalkMinutes { get; init; }
    public decimal? NearestCinemaDriveMinutes { get; init; }
    public decimal? NearestCinemaAnyWalkMinutes { get; init; }
    public decimal? NearestCinemaAnyDriveMinutes { get; init; }
    public int? NearestCinemaFoundRadiusM { get; init; }
    public int CafesCountPrimary { get; init; }
    public int RestaurantsCountPrimary { get; init; }
    public decimal? NearestMetroWalkMinutes { get; init; }
    public string? NearestMetroName { get; init; }
    public bool MetroFoundInPrimaryRadius { get; init; }
}

public sealed class DownstreamSellerPublicProfileDto
{
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public decimal? Rating { get; init; }
    public int? ReviewsCount { get; init; }
    public bool? IsVerified { get; init; }
    public string? AgencyName { get; init; }
}

public sealed class DownstreamSearchListingDto
{
    public Guid ListingId { get; init; }
    public decimal? InfrastructureScore { get; init; }
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];
}
