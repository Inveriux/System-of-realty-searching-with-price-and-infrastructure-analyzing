using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.OfficeClasses;

public record GetOfficeClassesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetOfficeClassesHandler(ILookupHandler<OfficeClass, LookupNameDto> lookupHandler)
    : IRequestHandler<GetOfficeClassesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetOfficeClassesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
