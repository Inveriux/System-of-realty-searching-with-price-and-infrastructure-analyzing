using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.HeatingTypes;

public record GetHeatingTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetHeatingTypesHandler(ILookupHandler<HeatingType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetHeatingTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetHeatingTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
