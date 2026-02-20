using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RoadAccessTypes;

public sealed record CreateRoadAccessTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateRoadAccessTypeHandler(ILookupHandler<RoadAccessType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateRoadAccessTypeCommand, Result>
{
    public async Task<Result> Handle(CreateRoadAccessTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
