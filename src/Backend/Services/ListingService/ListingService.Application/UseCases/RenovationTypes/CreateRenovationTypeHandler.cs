using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.RenovationTypes;

public sealed record CreateRenovationTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateRenovationTypeHandler(ILookupHandler<RenovationType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateRenovationTypeCommand, Result>
{
    public async Task<Result> Handle(CreateRenovationTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
