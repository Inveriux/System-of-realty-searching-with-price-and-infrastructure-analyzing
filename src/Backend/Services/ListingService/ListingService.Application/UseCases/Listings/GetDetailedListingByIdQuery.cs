using ListingService.Application.DTOs.Listings;
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

        if (listing is null)
        {
            return Result.NotFound<GetDetailedListingDto>(
                "Объявление не найдено.",
                [new ResultError("Объявление не найдено.", 0, nameof(request.Id))]);
        }

        return Result.Ok(mapper.Map<GetDetailedListingDto>(listing));
    }
}
