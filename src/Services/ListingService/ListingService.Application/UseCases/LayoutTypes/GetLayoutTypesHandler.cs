using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.LayoutTypes;

public record GetLayoutTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetLayoutTypesHandler(ILookupHandler<LayoutType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetLayoutTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetLayoutTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
