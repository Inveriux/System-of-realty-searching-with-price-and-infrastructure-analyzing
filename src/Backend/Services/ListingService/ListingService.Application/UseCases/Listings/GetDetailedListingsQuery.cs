using AutoMapper;
using BasicBuildingBlocks.Collections;
using ListingService.Application.Contracts.Repositories;
using ListingService.Application.DTOs.Listings;
using MediatR;

namespace ListingService.Application.UseCases.Listings;

public record GetDetailedListingsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<GetDetailedListingDto>>>;

public class GetDetailedListingsHandler(
    IRepositoryManager repositoryManager,
    IMapper mapper)
    : IRequestHandler<GetDetailedListingsQuery, Result<PagedResult<GetDetailedListingDto>>>
{
    public async Task<Result<PagedResult<GetDetailedListingDto>>> Handle(
        GetDetailedListingsQuery request,
        CancellationToken cancellationToken)
    {
        var listings = await repositoryManager.Listing.GetAllDetailedAsync(
            new PagedRequest(request.Page, request.PageSize),
            false,
            cancellationToken);

        return Result.Ok(listings.Map(listing => mapper.Map<GetDetailedListingDto>(listing)));
    }
}
