using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public sealed record CreateGasTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateGasTypeHandler(ILookupHandler<GasType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateGasTypeCommand, Result>
{
    public async Task<Result> Handle(CreateGasTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
