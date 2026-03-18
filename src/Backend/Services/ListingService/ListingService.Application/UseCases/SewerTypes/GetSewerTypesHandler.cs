using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public record GetSewerTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetSewerTypesHandler(ILookupHandler<SewerType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetSewerTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetSewerTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
