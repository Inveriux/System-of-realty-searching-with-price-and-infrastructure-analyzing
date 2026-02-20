using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public record GetWaterSupplyTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetWaterSupplyTypesHandler(ILookupHandler<WaterSupplyType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetWaterSupplyTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetWaterSupplyTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
