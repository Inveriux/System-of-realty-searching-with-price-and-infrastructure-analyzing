using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingMaterials;

public sealed record CreateBuildingMaterialCommand(string TypeName) : IRequest<Result>;

public sealed class CreateBuildingMaterialHandler(ILookupHandler<BuildingMaterial, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateBuildingMaterialCommand, Result>
{
    public async Task<Result> Handle(CreateBuildingMaterialCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
