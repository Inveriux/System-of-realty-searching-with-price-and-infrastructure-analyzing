using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public record GetSewerTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetSewerTypesHandler(ILookupHandler<SewerType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetSewerTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetSewerTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
