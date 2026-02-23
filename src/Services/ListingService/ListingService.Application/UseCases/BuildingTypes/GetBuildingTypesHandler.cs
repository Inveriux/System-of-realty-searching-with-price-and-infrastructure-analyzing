using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingTypes;

public record GetBuildingTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetBuildingTypesHandler(ILookupHandler<BuildingType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetBuildingTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetBuildingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
