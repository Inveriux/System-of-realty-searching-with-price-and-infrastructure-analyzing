using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.GasTypes;

public sealed record DeleteGasTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteGasTypeHandler(ILookupHandler<GasType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteGasTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteGasTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
