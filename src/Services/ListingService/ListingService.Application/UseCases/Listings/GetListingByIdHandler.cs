using ListingService.Application.DTOs.Listings;
using FluentResults;
using AutoMapper;
using MediatR;

namespace ListingService.Application.UseCases.Listings;
 
public record GetListingByIdQuery(Guid Id) : IRequest<Result<GetListingDto>>;

public class GetListingByIdHandler(IRepositoryManager repositoryManager,
    IMapper mapper)
    : IRequestHandler<GetListingByIdQuery, Result<GetListingDto>>
{
    public async Task<Result<GetListingDto>> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetByIdAsync(request.Id, false, cancellationToken);
        return Result.Ok(mapper.Map<GetListingDto>(listing));
    }
}