using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public sealed record DeleteLandCategoryCommand(string TypeName) : IRequest<Result>;

public sealed class DeleteLandCategoryHandler(ILookupHandler<LandCategory, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<DeleteLandCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteLandCategoryCommand request,
        CancellationToken cancellationToken)
        => await lookupHandler.DeleteByNameAsync(request.TypeName, cancellationToken);
}
