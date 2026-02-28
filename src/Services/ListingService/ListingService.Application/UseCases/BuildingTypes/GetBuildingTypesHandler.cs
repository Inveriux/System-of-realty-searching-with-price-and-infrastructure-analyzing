using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingTypes;

public record GetBuildingTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetBuildingTypesHandler(ILookupHandler<BuildingType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetBuildingTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetBuildingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
