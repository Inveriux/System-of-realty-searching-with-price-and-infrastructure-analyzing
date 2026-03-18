using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public record GetLandCategoriesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetLandCategoriesHandler(ILookupHandler<LandCategory, LookupNameDto> lookupHandler)
    : IRequestHandler<GetLandCategoriesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetLandCategoriesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
