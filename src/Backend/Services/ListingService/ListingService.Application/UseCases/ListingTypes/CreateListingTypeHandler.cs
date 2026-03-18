using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public sealed record CreateListingTypeCommand(string Name) : IRequest<Result>;

public sealed class CreateListingTypeHandler(ILookupHandler<ListingType, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateListingTypeCommand, Result>
{
    public async Task<Result> Handle(CreateListingTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
