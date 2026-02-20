using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public sealed record DeleteSewerTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteSewerTypeHandler(ILookupHandler<SewerType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteSewerTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteSewerTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
