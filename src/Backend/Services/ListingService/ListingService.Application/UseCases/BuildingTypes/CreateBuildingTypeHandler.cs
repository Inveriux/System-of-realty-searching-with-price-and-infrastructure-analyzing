using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingTypes;

public sealed record CreateBuildingTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateBuildingTypeHandler(ILookupHandler<BuildingType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateBuildingTypeCommand, Result>
{
    public async Task<Result> Handle(CreateBuildingTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
