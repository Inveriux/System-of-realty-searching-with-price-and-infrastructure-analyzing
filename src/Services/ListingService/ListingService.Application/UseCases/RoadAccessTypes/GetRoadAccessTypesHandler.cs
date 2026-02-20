using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RoadAccessTypes;

public record GetRoadAccessTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetRoadAccessTypesHandler(ILookupHandler<RoadAccessType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetRoadAccessTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetRoadAccessTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
