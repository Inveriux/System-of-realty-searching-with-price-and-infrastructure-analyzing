using SearchService.DTOs.Search;

namespace SearchService.Services.Ranking;

public sealed class SearchProfileProvider : ISearchProfileProvider
{
    private const string DefaultProfile = "default";

    public string GetResolvedProfile(SearchListingsRequestDto request)
        => string.IsNullOrWhiteSpace(request.SearchProfile)
            ? DefaultProfile
            : request.SearchProfile.Trim().ToLowerInvariant();

    public SearchRankingWeightsDto GetWeights(SearchListingsRequestDto request)
    {
        var preset = GetPreset(GetResolvedProfile(request));

        return new SearchRankingWeightsDto
        {
            TransportWeight = request.TransportWeight ?? preset.TransportWeight,
            EducationWeight = request.EducationWeight ?? preset.EducationWeight,
            HealthcareWeight = request.HealthcareWeight ?? preset.HealthcareWeight,
            CommerceWeight = request.CommerceWeight ?? preset.CommerceWeight,
            LeisureWeight = request.LeisureWeight ?? preset.LeisureWeight,
            PriceWeight = request.PriceWeight ?? preset.PriceWeight,
            LocationWeight = request.LocationWeight ?? preset.LocationWeight,
            AreaWeight = request.AreaWeight ?? preset.AreaWeight
        };
    }

    private static SearchRankingWeightsDto GetPreset(string profile)
        => profile switch
        {
            "family" => new SearchRankingWeightsDto
            {
                TransportWeight = 0.95m,
                EducationWeight = 1.45m,
                HealthcareWeight = 1.20m,
                CommerceWeight = 0.95m,
                LeisureWeight = 1.15m,
                PriceWeight = 1.00m,
                LocationWeight = 1.00m,
                AreaWeight = 1.10m
            },
            "urban" => new SearchRankingWeightsDto
            {
                TransportWeight = 1.40m,
                EducationWeight = 0.85m,
                HealthcareWeight = 1.00m,
                CommerceWeight = 1.20m,
                LeisureWeight = 1.35m,
                PriceWeight = 0.90m,
                LocationWeight = 1.20m,
                AreaWeight = 0.85m
            },
            "investment" => new SearchRankingWeightsDto
            {
                TransportWeight = 1.20m,
                EducationWeight = 0.80m,
                HealthcareWeight = 0.90m,
                CommerceWeight = 1.15m,
                LeisureWeight = 0.95m,
                PriceWeight = 1.50m,
                LocationWeight = 1.00m,
                AreaWeight = 0.80m
            },
            "student" => new SearchRankingWeightsDto
            {
                TransportWeight = 1.35m,
                EducationWeight = 0.85m,
                HealthcareWeight = 1.00m,
                CommerceWeight = 1.10m,
                LeisureWeight = 1.20m,
                PriceWeight = 1.20m,
                LocationWeight = 1.00m,
                AreaWeight = 0.85m
            },
            _ => new SearchRankingWeightsDto
            {
                TransportWeight = 1.00m,
                EducationWeight = 1.00m,
                HealthcareWeight = 1.00m,
                CommerceWeight = 1.00m,
                LeisureWeight = 1.00m,
                PriceWeight = 1.00m,
                LocationWeight = 1.00m,
                AreaWeight = 1.00m
            }
        };
}
