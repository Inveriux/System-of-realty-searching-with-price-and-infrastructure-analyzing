using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.HeatingTypes;

public sealed record CreateHeatingTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateHeatingTypeHandler(ILookupHandler<HeatingType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateHeatingTypeCommand, Result>
{
    public async Task<Result> Handle(CreateHeatingTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
