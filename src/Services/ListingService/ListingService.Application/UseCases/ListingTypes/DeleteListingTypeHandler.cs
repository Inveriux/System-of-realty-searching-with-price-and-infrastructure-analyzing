using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public sealed record DeleteListingTypeCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteListingTypeHandler(ILookupHandler<ListingType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteListingTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteListingTypeCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
