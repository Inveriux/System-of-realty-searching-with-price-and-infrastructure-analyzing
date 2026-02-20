using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public record GetListingTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetListingTypesHandler(ILookupHandler<ListingType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetListingTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetListingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
