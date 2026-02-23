using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public record GetLayoutTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetLayoutTypesHandler(ILookupHandler<LayoutType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetLayoutTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetLayoutTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
