using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public record GetLayoutTypesQuery() : IRequest<Result<PagedResult<LookupClassNameDto>>>;

public class GetLayoutTypesHandler(ILookupHandler<LayoutType, LookupClassNameDto> lookupHandler)
    : IRequestHandler<GetLayoutTypesQuery, Result<PagedResult<LookupClassNameDto>>>
{
    public async Task<Result<PagedResult<LookupClassNameDto>>> Handle(GetLayoutTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
