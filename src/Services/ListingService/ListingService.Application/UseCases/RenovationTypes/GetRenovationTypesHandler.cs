using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RenovationTypes;

public record GetRenovationTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetRenovationTypesHandler(ILookupHandler<RenovationType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetRenovationTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetRenovationTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
