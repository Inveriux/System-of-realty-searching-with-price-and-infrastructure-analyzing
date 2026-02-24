using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public record GetListingTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetListingTypesHandler(ILookupHandler<ListingType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetListingTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetListingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
