using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.WaterSupplyTypes;

public sealed record CreateWaterSupplyTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateWaterSupplyTypeHandler(ILookupHandler<WaterSupplyType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateWaterSupplyTypeCommand, Result>
{
    public async Task<Result> Handle(CreateWaterSupplyTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
