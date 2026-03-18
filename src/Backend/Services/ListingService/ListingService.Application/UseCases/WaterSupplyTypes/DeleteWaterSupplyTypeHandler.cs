using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public sealed record DeleteWaterSupplyTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteWaterSupplyTypeHandler(ILookupHandler<WaterSupplyType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteWaterSupplyTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteWaterSupplyTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
