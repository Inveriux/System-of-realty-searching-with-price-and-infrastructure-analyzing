using AutoMapper;
using SearchService.Models;
using System.Text.Json;

namespace SearchService.DTOs.Search;

public sealed class SearchListingDto
{
    public Guid ListingId { get; init; }
    public Guid AgentId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AddressText { get; init; }
    public Guid? LocationId { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? ListingType { get; init; }
    public string? BuildingType { get; init; }
    public string? ListingDetailsType { get; init; }
    public decimal? PriceAmount { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal AreaTotal { get; init; }
    public string? MainMediaUrl { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public decimal? InfrastructureScore { get; init; }
    public decimal? EducationScore { get; init; }
    public decimal? HealthcareScore { get; init; }
    public decimal? TransportScore { get; init; }
    public decimal? CommerceScore { get; init; }
    public decimal? LeisureScore { get; init; }
    public int? EducationCount { get; init; }
    public int? HealthcareCount { get; init; }
    public int? TransportCount { get; init; }
    public int? CommerceCount { get; init; }
    public int? LeisureCount { get; init; }
    public decimal? EducationNearestMeters { get; init; }
    public decimal? HealthcareNearestMeters { get; init; }
    public decimal? TransportNearestMeters { get; init; }
    public decimal? CommerceNearestMeters { get; init; }
    public decimal? LeisureNearestMeters { get; init; }
    public int? PharmacyCountPrimary { get; init; }
    public int? StoreCountPrimary { get; init; }
    public int? StopsCountPrimary { get; init; }
    public int? StopGroupsCountPrimary { get; init; }
    public int? StopPlatformsCountPrimary { get; init; }
    public int? ParkingCountPrimary { get; init; }
    public int? SchoolCountPrimary { get; init; }
    public int? KindergartenCountPrimary { get; init; }
    public int? ClinicCountPrimary { get; init; }
    public int? HospitalCountPrimary { get; init; }
    public decimal? NearestPharmacyWalkMinutes { get; init; }
    public decimal? NearestStoreWalkMinutes { get; init; }
    public decimal? NearestStopWalkMinutes { get; init; }
    public decimal? NearestStopGroupWalkMinutes { get; init; }
    public decimal? NearestStopGroupDriveMinutes { get; init; }
    public bool NearestStopGroupDriveMinutesEstimated { get; init; }
    public string? NearestStopGroupName { get; init; }
    public int? NearestStopGroupPlatformCount { get; init; }
    public decimal? NearestParkingWalkMinutes { get; init; }
    public decimal? NearestParkingDriveMinutes { get; init; }
    public bool NearestParkingDriveMinutesEstimated { get; init; }
    public decimal? NearestSchoolWalkMinutes { get; init; }
    public decimal? NearestKindergartenWalkMinutes { get; init; }
    public decimal? NearestClinicDriveMinutes { get; init; }
    public bool NearestClinicDriveMinutesEstimated { get; init; }
    public decimal? NearestHospitalDriveMinutes { get; init; }
    public bool NearestHospitalDriveMinutesEstimated { get; init; }
    public int? ParksCountPrimary { get; init; }
    public decimal? NearestParkWalkMinutes { get; init; }
    public int? FitnessCountPrimary { get; init; }
    public decimal? NearestFitnessWalkMinutes { get; init; }
    public decimal? NearestFitnessDriveMinutes { get; init; }
    public int? MuseumsCountPrimary { get; init; }
    public decimal? NearestMuseumDriveMinutes { get; init; }
    public int? TheatersCountPrimary { get; init; }
    public decimal? NearestTheaterDriveMinutes { get; init; }
    public int? CinemasCountPrimary { get; init; }
    public decimal? NearestCinemaDriveMinutes { get; init; }
    public decimal? NearestMetroWalkMinutes { get; init; }
    public string? NearestMetroName { get; init; }
    public bool MetroFoundInPrimaryRadius { get; init; }
    public decimal? RelevanceScore { get; set; }
    public SearchRelevanceBreakdownDto? RelevanceBreakdown { get; set; }
    public IReadOnlyCollection<string> Highlights { get; set; } = [];
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];

    private sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SearchListing, SearchListingDto>()
                .ForMember(x => x.SearchFeatures, y => y.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.SearchFeaturesJson)
                        ? new Dictionary<string, decimal>()
                        : JsonSerializer.Deserialize<Dictionary<string, decimal>>(s.SearchFeaturesJson) ?? new Dictionary<string, decimal>()));
        }
    }
}
