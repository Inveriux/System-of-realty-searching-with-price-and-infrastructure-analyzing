using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Pages;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ClassTypes;

public record GetClassTypesQuery() : IRequest<Result<PagedResult<LookupTypeNameDto>>>;

public class GetClassTypesHandler(ILookupHandler<ClassType, LookupTypeNameDto> lookupHandler)
    : IRequestHandler<GetClassTypesQuery, Result<PagedResult<LookupTypeNameDto>>>
{
    public async Task<Result<PagedResult<LookupTypeNameDto>>> Handle(GetClassTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
