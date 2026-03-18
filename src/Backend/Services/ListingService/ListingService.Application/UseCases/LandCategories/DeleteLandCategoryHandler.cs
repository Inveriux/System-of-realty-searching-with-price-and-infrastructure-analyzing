using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public sealed record DeleteLandCategoryCommand(string Name) : IRequest<Result>;

public sealed class DeleteLandCategoryHandler(ILookupHandler<LandCategory, LookupNameDto> lookupHandler)
    : IRequestHandler<DeleteLandCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteLandCategoryCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.Name, cancellationToken);
}
