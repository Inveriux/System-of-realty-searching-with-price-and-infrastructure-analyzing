using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BathroomTypes;

public record GetBathroomTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetBathroomTypesHandler(ILookupHandler<BathroomType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetBathroomTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetBathroomTypesQuery request, 
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);

}