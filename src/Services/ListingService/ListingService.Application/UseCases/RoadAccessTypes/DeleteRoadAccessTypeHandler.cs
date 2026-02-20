using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RoadAccessTypes;

public sealed record DeleteRoadAccessTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteRoadAccessTypeHandler(ILookupHandler<RoadAccessType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteRoadAccessTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteRoadAccessTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
