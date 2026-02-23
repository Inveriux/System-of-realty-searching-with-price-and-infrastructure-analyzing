using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RenovationTypes;

public sealed record DeleteRenovationTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteRenovationTypeHandler(ILookupHandler<RenovationType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteRenovationTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteRenovationTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
