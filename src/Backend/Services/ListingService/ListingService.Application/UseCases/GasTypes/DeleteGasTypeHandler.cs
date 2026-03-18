using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public sealed record DeleteGasTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteGasTypeHandler(ILookupHandler<GasType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteGasTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteGasTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
