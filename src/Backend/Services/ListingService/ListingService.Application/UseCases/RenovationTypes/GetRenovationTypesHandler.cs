using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.RenovationTypes;

public record GetRenovationTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetRenovationTypesHandler(ILookupHandler<RenovationType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetRenovationTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetRenovationTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
