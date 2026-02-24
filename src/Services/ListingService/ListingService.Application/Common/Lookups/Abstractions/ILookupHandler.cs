using ListingService.Application.DTOs.Lookups.Abstractions;
using BasicBuildingBlocks.Collections;
using FluentResults;

namespace ListingService.Application.Common.Lookups.Abstractions;

public interface ILookupHandler<TLookup, TLookupDto> where TLookup : class 
    where TLookupDto : ILookupDto
{
    Task<Result<PagedResult<TLookupDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result> CreateAsync(TLookupDto lookupDto, CancellationToken cancellationToken);
    Task<Result> DeleteByNameAsync(string name, CancellationToken cancellationToken);
}
