using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RoadAccessTypes;

public record GetRoadAccessTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetRoadAccessTypesHandler(ILookupHandler<RoadAccessType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetRoadAccessTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetRoadAccessTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
