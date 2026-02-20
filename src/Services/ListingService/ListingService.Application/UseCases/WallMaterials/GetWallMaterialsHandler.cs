using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WallMaterials;

public record GetWallMaterialsQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetWallMaterialsHandler(ILookupHandler<WallMaterial, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetWallMaterialsQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetWallMaterialsQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
