using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.OfficeClasses;

public record GetOfficeClassesQuery() : IRequest<Result<PagedResult<LookupClassNameDto>>>;

public class GetOfficeClassesHandler(ILookupHandler<OfficeClass, LookupClassNameDto> lookupHandler)
    : IRequestHandler<GetOfficeClassesQuery, Result<PagedResult<LookupClassNameDto>>>
{
    public async Task<Result<PagedResult<LookupClassNameDto>>> Handle(GetOfficeClassesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
