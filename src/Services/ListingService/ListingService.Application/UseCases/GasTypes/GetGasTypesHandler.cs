using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public record GetGasTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetGasTypesHandler(ILookupHandler<GasType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetGasTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetGasTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
