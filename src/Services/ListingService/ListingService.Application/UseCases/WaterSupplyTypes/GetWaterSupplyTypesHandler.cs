using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public record GetWaterSupplyTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetWaterSupplyTypesHandler(ILookupHandler<WaterSupplyType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetWaterSupplyTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetWaterSupplyTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
