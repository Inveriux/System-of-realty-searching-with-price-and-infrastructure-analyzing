using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public sealed record CreateLandCategoryCommand(string Name) : IRequest<Result>;

public sealed class CreateLandCategoryHandler(ILookupHandler<LandCategory, LookupNameDto> lookupHandler)
    : IRequestHandler<CreateLandCategoryCommand, Result>
{
    public async Task<Result> Handle(CreateLandCategoryCommand request, CancellationToken cancellationToken)
        => await lookupHandler.CreateAsync(new LookupNameDto { Name = request.Name }, cancellationToken);
}
