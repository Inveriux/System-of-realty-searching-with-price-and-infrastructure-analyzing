using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WallMaterials;

public sealed record DeleteWallMaterialCommand(string Name) : IRequest<Result>;

public sealed class DeleteWallMaterialHandler(ILookupHandler<WallMaterial, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteWallMaterialCommand, Result>
{
    public async Task<Result> Handle(DeleteWallMaterialCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
