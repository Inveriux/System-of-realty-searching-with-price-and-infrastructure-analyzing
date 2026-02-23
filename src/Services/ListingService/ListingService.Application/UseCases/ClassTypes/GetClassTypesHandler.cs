using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ClassTypes;

public record GetClassTypesQuery() : IRequest<Result<PagedResult<LookupNameDto>>>;

public class GetClassTypesHandler(ILookupHandler<ClassType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetClassTypesQuery, Result<PagedResult<LookupNameDto>>>
{
    public async Task<Result<PagedResult<LookupNameDto>>> Handle(GetClassTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
