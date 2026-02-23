using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.HeatingTypes;

public sealed record DeleteHeatingTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteHeatingTypeHandler(ILookupHandler<HeatingType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteHeatingTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteHeatingTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
