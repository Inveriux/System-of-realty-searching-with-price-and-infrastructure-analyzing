using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.DTOs.Lookups;
using ListingService.Domain.Models;
using MediatR;

namespace ListingService.Application.UseCases.OfficeClasses;

public record GetOfficeClassesQuery() : IRequest<Result<List<LookupNameDto>>>;

public class GetOfficeClassesHandler(ILookupHandler<OfficeClass, LookupNameDto> lookupHandler)
    : IRequestHandler<GetOfficeClassesQuery, Result<List<LookupNameDto>>>
{
    public async Task<Result<List<LookupNameDto>>> Handle(GetOfficeClassesQuery request,
        CancellationToken cancellationToken)
        => await lookupHandler.GetAllAsync(cancellationToken);
}
