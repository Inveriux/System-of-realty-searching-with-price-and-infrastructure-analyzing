using AutoMapper;
using SearchService.Models;
using System.Text.Json;

namespace SearchService.DTOs.Search;

public sealed class SearchListingDto
{
    public Guid ListingId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AddressText { get; init; }
    public Guid? LocationId { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? ListingType { get; init; }
    public string? BuildingType { get; init; }
    public decimal? PriceAmount { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal AreaTotal { get; init; }
    public string? MainMediaUrl { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public decimal? InfrastructureScore { get; init; }
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
