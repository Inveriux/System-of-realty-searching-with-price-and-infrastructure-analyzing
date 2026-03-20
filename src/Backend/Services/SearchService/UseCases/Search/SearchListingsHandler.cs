using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using SearchService.DTOs.Search;
using MediatR;

namespace SearchService.UseCases.Search;

public sealed record SearchListingsQuery(SearchListingsRequestDto Request) : IRequest<Result<SearchListingsResponseDto>>;

public sealed class SearchListingsHandler(ISearchListingRepository repository, IMapper mapper)
    : IRequestHandler<SearchListingsQuery, Result<SearchListingsResponseDto>>
{
    public async Task<Result<SearchListingsResponseDto>> Handle(SearchListingsQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.SearchAsync(
            request.Request.Query,
            request.Request.LocationId,
            request.Request.ListingType,
            request.Request.MinPrice,
            request.Request.MaxPrice,
            request.Request.Page,
            request.Request.PageSize,
            cancellationToken);

        return Result.Ok(new SearchListingsResponseDto
        {
            Items = mapper.Map<IReadOnlyCollection<SearchListingDto>>(result.Items),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }
}
