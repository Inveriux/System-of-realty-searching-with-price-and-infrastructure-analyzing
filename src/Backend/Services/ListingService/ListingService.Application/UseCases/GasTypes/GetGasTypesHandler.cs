using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public record GetGasTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetGasTypesHandler(ILookupHandler<GasType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetGasTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetGasTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
