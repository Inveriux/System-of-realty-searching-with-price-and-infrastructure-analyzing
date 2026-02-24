using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public record GetLandCategoriesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetLandCategoriesHandler(ILookupHandler<LandCategory, LookupNameDto> lookupHandler)
    : IRequestHandler<GetLandCategoriesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetLandCategoriesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
