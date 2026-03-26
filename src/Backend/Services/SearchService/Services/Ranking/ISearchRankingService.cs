using SearchService.DTOs.Search;
using SearchService.Models;

namespace SearchService.Services.Ranking;

public interface ISearchRankingService
{
    SearchRankingResult Rank(SearchListing listing, SearchListingsRequestDto request, SearchRankingWeightsDto weights);
}
