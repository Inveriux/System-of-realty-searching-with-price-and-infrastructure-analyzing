using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.BathroomTypes;

public record GetBathroomTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetBathroomTypesHandler(ILookupHandler<BathroomType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetBathroomTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetBathroomTypesQuery request, 
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}