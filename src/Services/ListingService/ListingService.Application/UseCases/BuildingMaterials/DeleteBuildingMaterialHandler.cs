using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingMaterials;

public sealed record DeleteBuildingMaterialCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteBuildingMaterialHandler(ILookupHandler<BuildingMaterial, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteBuildingMaterialCommand, Result>
{
    public async Task<Result> Handle(DeleteBuildingMaterialCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
