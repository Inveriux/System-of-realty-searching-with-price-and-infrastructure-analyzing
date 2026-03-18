using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public sealed record DeleteSewerTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteSewerTypeHandler(ILookupHandler<SewerType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteSewerTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteSewerTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
