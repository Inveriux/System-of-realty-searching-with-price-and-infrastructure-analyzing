using System.Text.Json;
using SearchService.Models;
using SearchService.Services.InternalClients.Contracts;

namespace SearchService.UseCases.Indexing;

internal static class SearchListingInfrastructureMapper
{
    public static void Apply(SearchListing listing, InfrastructureScorePayloadDto? infrastructureScore)
    {
        var features = infrastructureScore?.SearchFeatures ?? [];

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
        listing.SearchFeaturesJson = JsonSerializer.Serialize(features);
    }

    private static int? GetInt(IReadOnlyDictionary<string, decimal> features, string key)
        => features.TryGetValue(key, out var value) ? (int)value : null;

    private static decimal? GetDecimal(IReadOnlyDictionary<string, decimal> features, string key)
        => features.TryGetValue(key, out var value) ? value : null;
}
