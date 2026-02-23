using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WallMaterials;

public sealed record CreateWallMaterialCommand(string Name) : IRequest<Result>;

public sealed class CreateWallMaterialHandler(ILookupHandler<WallMaterial, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateWallMaterialCommand, Result>
{
    public async Task<Result> Handle(CreateWallMaterialCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
