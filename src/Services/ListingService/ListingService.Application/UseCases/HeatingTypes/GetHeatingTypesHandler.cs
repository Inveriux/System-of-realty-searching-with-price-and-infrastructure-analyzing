using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.HeatingTypes;

public record GetHeatingTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetHeatingTypesHandler(ILookupHandler<HeatingType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetHeatingTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetHeatingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
