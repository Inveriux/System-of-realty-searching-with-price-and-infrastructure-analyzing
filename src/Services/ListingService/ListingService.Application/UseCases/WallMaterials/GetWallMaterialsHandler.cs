using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WallMaterials;

public record GetWallMaterialsQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetWallMaterialsHandler(ILookupHandler<WallMaterial, LookupNameDto> lookupHandler)
    : IRequestHandler<GetWallMaterialsQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetWallMaterialsQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
