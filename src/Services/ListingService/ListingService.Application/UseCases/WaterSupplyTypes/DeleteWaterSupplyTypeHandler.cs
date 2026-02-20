using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public sealed record DeleteWaterSupplyTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteWaterSupplyTypeHandler(ILookupHandler<WaterSupplyType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteWaterSupplyTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteWaterSupplyTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
