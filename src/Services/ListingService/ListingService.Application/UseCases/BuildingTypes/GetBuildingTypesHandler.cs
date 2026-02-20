using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BuildingTypes;

public record GetBuildingTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetBuildingTypesHandler(ILookupHandler<BuildingType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetBuildingTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetBuildingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
