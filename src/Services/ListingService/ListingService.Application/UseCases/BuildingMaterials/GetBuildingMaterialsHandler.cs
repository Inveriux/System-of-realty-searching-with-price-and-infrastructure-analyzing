using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingMaterials;

public record GetBuildingMaterialsQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetBuildingMaterialsHandler(ILookupHandler<BuildingMaterial, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetBuildingMaterialsQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetBuildingMaterialsQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
