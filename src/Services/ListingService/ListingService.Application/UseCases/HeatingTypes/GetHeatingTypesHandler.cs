using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.HeatingTypes;

public record GetHeatingTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetHeatingTypesHandler(ILookupHandler<HeatingType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetHeatingTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetHeatingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
