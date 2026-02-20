using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LandCategories;

public record GetLandCategoriesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetLandCategoriesHandler(ILookupHandler<LandCategory, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetLandCategoriesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetLandCategoriesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
