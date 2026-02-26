using ListingService.Application.DTOs.Listings;
using BasicBuildingBlocks.Collections;
using FluentResults;
using AutoMapper;
using MediatR;

namespace ListingService.Application.UseCases.Listings;
 
public record GetListingQuery(int Page, int PageSize) : IRequest<Result<PagedResult<GetListingDto>>>;

public class GetListingHandler(IRepositoryManager repositoryManager,
    IMapper mapper)
    : IRequestHandler<GetListingQuery, Result<PagedResult<GetListingDto>>>
{
    public async Task<Result<PagedResult<GetListingDto>>> Handle(GetListingQuery request, CancellationToken cancellationToken)
    {   
        var listings = await repositoryManager.Listing.GetAllAsync(new(request.Page, 
            request.PageSize), 
            false, 
            cancellationToken);

        return Result.Ok(listings.Map(l => mapper.Map<GetListingDto>(l)));
    }
}