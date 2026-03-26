using SearchService.DTOs.Search;

namespace SearchService.Services.Ranking;

public interface ISearchProfileProvider
{
    SearchRankingWeightsDto GetWeights(SearchListingsRequestDto request);
    string GetResolvedProfile(SearchListingsRequestDto request);
}
