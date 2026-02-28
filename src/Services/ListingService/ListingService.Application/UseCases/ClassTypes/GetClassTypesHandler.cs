using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using FluentResults;
using MediatR;

namespace ListingService.Application.UseCases.ClassTypes;

public record GetClassTypesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetClassTypesHandler(ILookupHandler<ClassType, LookupNameDto> lookupHandler)
    : IRequestHandler<GetClassTypesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetClassTypesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
