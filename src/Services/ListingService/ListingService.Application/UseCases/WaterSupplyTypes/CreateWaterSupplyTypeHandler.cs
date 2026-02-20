using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public sealed record CreateWaterSupplyTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateWaterSupplyTypeHandler(ILookupHandler<WaterSupplyType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateWaterSupplyTypeCommand, Result>
{
    public async Task<Result> Handle(CreateWaterSupplyTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
