using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.SewerTypes;

public record GetSewerTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetSewerTypesHandler(ILookupHandler<SewerType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetSewerTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetSewerTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
