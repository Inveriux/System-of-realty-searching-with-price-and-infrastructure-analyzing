using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BathroomTypes;

public record GetBathroomTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetBathroomTypesHandler(ILookupHandler<BathroomType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetBathroomTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetBathroomTypesQuery request, 
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);

}