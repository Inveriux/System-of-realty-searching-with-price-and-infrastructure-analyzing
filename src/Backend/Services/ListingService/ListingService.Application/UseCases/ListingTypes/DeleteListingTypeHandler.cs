using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public sealed record DeleteListingTypeCommand(string Name) : IRequest<Result>;

public sealed class DeleteListingTypeHandler(ILookupHandler<ListingType, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteListingTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteListingTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
