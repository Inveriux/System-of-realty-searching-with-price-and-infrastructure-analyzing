using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.RoadAccessTypes;

public sealed record CreateRoadAccessTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateRoadAccessTypeHandler(ILookupHandler<RoadAccessType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateRoadAccessTypeCommand, Result>
{
    public async Task<Result> Handle(CreateRoadAccessTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
