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
