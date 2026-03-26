using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using MediatR;
using SearchService.Data.Repositories.Abstractions;
using SearchService.DTOs.Search;
using SearchService.Models;
using SearchService.Services.Ranking;

namespace SearchService.UseCases.Search;

public sealed record SearchListingsQuery(SearchListingsRequestDto Request) : IRequest<Result<SearchListingsResponseDto>>;

public sealed class SearchListingsHandler(
    ISearchListingRepository repository,
    ISearchRankingService rankingService,
    ISearchProfileProvider profileProvider,
    IMapper mapper)
    : IRequestHandler<SearchListingsQuery, Result<SearchListingsResponseDto>>
{
    public async Task<Result<SearchListingsResponseDto>> Handle(SearchListingsQuery request, CancellationToken cancellationToken)
    {
        var sort = NormalizeSort(request.Request);
        var resolvedProfile = profileProvider.GetResolvedProfile(request.Request);
        var weights = profileProvider.GetWeights(request.Request);

        if (sort == "relevance")
        {
            var candidates = await repository.GetRelevanceCandidatesAsync(request.Request, cancellationToken);
            var ranked = candidates.Items
                .Select(listing => new RankedListing(listing, rankingService.Rank(listing, request.Request, weights)))
                .OrderByDescending(x => x.Ranking.FinalScore)
                .ThenByDescending(x => x.Listing.PublishedDate)
                .Skip(request.Request.Page * request.Request.PageSize)
                .Take(request.Request.PageSize)
                .Select(x => MapRankedDto(x.Listing, x.Ranking, request.Request, mapper))
                .ToArray();

            return Result.Ok(new SearchListingsResponseDto
            {
                Items = ranked,
                Sort = sort,
                SearchProfile = resolvedProfile,
                AppliedWeights = weights,
                Page = request.Request.Page,
                PageSize = request.Request.PageSize,
                TotalCount = candidates.TotalCount
            });
        }

        var page = await repository.SearchAsync(request.Request, cancellationToken);
        var pageItems = page.Items
            .Select(listing => new RankedListing(listing, rankingService.Rank(listing, request.Request, weights)))
            .Select(x => MapRankedDto(x.Listing, x.Ranking, request.Request, mapper))
            .ToArray();

        return Result.Ok(new SearchListingsResponseDto
        {
            Items = pageItems,
            Sort = sort,
            SearchProfile = resolvedProfile,
            AppliedWeights = weights,
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        });
    }

    private static SearchListingDto MapRankedDto(
        SearchListing listing,
        SearchRankingResult ranking,
        SearchListingsRequestDto request,
        IMapper mapper)
    {
        var dto = mapper.Map<SearchListingDto>(listing);
        dto.RelevanceScore = ranking.FinalScore;
        dto.RelevanceBreakdown = request.IncludeRelevanceBreakdown ? ranking.Breakdown : null;
        dto.Highlights = request.IncludeHighlights ? ranking.Highlights : Array.Empty<string>();
        return dto;
    }

    private static string NormalizeSort(SearchListingsRequestDto request)
        => ((request.Sort ?? request.SortBy) ?? "published_desc").Trim().ToLowerInvariant();

    private sealed record RankedListing(SearchListing Listing, SearchRankingResult Ranking);
}
