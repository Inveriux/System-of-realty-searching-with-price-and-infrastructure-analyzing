using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ListingTypes;

public sealed record CreateListingTypeCommand(string TypeName) : IRequest<Result>;

public sealed class CreateListingTypeHandler(ILookupHandler<ListingType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateListingTypeCommand, Result>
{
    public async Task<Result> Handle(CreateListingTypeCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
