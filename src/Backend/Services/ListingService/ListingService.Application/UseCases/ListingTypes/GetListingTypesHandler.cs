using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public record GetListingTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetListingTypesHandler(ILookupHandler<ListingType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetListingTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetListingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
