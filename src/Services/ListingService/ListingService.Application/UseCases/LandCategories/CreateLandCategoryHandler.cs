using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public sealed record CreateLandCategoryCommand(string TypeName) : IRequest<Result>;

public sealed class CreateLandCategoryHandler(ILookupHandler<LandCategory, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<CreateLandCategoryCommand, Result>
{
    public async Task<Result> Handle(CreateLandCategoryCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(request.TypeName, cancellationToken);
}
