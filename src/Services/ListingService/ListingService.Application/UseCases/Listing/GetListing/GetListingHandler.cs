using ListingService.Application.DTOs.Listing;
using AutoMapper;
using MediatR;

namespace ListingService.Application.UseCases.Listing.GetListing;
 
public record GetListingQuery() : IRequest<IEnumerable<GetListingDto>>;

public class GetListingHandler(IRepositoryManager repositoryManager,
    IMapper mapper)
    : IRequestHandler<GetListingQuery, IEnumerable<GetListingDto>>
{
    public async Task<IEnumerable<GetListingDto>> Handle(GetListingQuery request, CancellationToken cancellationToken)
    {   
        var listing = await repositoryManager.Listing.GetAllAsync(new(0, 0), false, cancellationToken);

        return mapper.Map<IEnumerable<GetListingDto>>(listing);
    }
}