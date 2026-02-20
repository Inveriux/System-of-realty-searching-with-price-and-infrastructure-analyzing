using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.RenovationTypes;

public record GetRenovationTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetRenovationTypesHandler(ILookupHandler<RenovationType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetRenovationTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetRenovationTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
