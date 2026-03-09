using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WallMaterials;

public record GetWallMaterialsQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetWallMaterialsHandler(ILookupHandler<WallMaterial, LookupNameDto> lookupHandler)
    : IRequestHandler<GetWallMaterialsQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetWallMaterialsQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
