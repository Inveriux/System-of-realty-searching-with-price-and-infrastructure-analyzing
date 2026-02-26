using ListingService.Application.DTOs.Listings;
using FluentResults;
using AutoMapper;
using MediatR;

namespace ListingService.Application.UseCases.Listings;
 
public record GetDetailedListingByIdQuery(Guid Id) : IRequest<Result<GetDetailedListingDto>>;

public class GetDetailedListingByIdHandler(IRepositoryManager repositoryManager,
    IMapper mapper)
    : IRequestHandler<GetDetailedListingByIdQuery, Result<GetDetailedListingDto>>
{
    public async Task<Result<GetDetailedListingDto>> Handle(GetDetailedListingByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetDetailedByIdAsync(request.Id, 
            false, 
            cancellationToken);
        return Result.Ok(mapper.Map<GetDetailedListingDto>(listing));
    }
}