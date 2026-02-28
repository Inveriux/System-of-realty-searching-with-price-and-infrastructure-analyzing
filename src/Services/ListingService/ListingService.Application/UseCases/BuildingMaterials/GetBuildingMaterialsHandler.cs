using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingMaterials;

public record GetBuildingMaterialsQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetBuildingMaterialsHandler(ILookupHandler<BuildingMaterial, LookupNameDto> lookupHandler)
    : IRequestHandler<GetBuildingMaterialsQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetBuildingMaterialsQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
