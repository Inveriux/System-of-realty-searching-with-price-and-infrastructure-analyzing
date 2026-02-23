using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingMaterials;

public record GetBuildingMaterialsQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetBuildingMaterialsHandler(ILookupHandler<BuildingMaterial, LookupNameDto> lookupHandler)
    : IRequestHandler<GetBuildingMaterialsQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetBuildingMaterialsQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
