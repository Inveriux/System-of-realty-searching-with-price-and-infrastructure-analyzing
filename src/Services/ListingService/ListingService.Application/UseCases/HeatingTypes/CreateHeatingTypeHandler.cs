using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.HeatingTypes;

public sealed record CreateHeatingTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateHeatingTypeHandler(ILookupHandler<HeatingType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateHeatingTypeCommand, Result>
{
    public async Task<Result> Handle(CreateHeatingTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
