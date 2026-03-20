using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using SearchService.DTOs.Search;
using MediatR;

namespace SearchService.UseCases.Search;

public sealed record GetSearchListingByIdQuery(Guid ListingId) : IRequest<Result<SearchListingDto>>;

public sealed class GetSearchListingByIdHandler(ISearchListingRepository repository, IMapper mapper)
    : IRequestHandler<GetSearchListingByIdQuery, Result<SearchListingDto>>
{
    public async Task<Result<SearchListingDto>> Handle(GetSearchListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByListingIdAsync(request.ListingId, cancellationToken);
        return listing is null
            ? Result.NotFound<SearchListingDto>("Indexed listing was not found.")
            : Result.Ok(mapper.Map<SearchListingDto>(listing));
    }
}
