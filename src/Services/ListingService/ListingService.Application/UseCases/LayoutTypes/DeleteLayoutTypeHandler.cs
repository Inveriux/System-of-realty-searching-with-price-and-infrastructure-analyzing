using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public sealed record DeleteLayoutTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteLayoutTypeHandler(ILookupHandler<LayoutType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteLayoutTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteLayoutTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
