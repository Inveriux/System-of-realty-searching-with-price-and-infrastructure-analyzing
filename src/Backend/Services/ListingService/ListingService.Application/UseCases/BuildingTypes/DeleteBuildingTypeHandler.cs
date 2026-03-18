using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.BuildingTypes;

public sealed record DeleteBuildingTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteBuildingTypeHandler(ILookupHandler<BuildingType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteBuildingTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteBuildingTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
