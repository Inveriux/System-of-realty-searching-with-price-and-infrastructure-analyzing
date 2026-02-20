using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public record GetGasTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetGasTypesHandler(ILookupHandler<GasType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetGasTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetGasTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
