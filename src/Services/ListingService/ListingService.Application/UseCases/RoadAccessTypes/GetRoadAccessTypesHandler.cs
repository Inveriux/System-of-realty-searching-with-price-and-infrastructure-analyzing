using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RoadAccessTypes;

public record GetRoadAccessTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetRoadAccessTypesHandler(ILookupHandler<RoadAccessType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetRoadAccessTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetRoadAccessTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
