using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public record GetWaterSupplyTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetWaterSupplyTypesHandler(ILookupHandler<WaterSupplyType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetWaterSupplyTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetWaterSupplyTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
