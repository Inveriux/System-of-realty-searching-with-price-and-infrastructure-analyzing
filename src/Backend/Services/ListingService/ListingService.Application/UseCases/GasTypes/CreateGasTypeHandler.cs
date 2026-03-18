using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public sealed record CreateGasTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateGasTypeHandler(ILookupHandler<GasType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateGasTypeCommand, Result>
{
    public async Task<Result> Handle(CreateGasTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
