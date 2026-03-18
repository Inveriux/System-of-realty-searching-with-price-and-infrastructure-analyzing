using ListingService.Application.DTOs.Listings;
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

        if (listing is null)
        {
            return Result.NotFound<GetListingDto>(
                "Listing not found.",
                [new ResultError("Listing not found.", 0, nameof(request.Id))]);
        }

        return Result.Ok(mapper.Map<GetListingDto>(listing));
    }
}
