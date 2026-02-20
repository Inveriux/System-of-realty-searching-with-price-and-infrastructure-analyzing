using BasicBuildingBlocks.Pages;
using FluentResults;

namespace ListingService.Application.Common.Lookups.Abstractions;

public interface ILookupHandler<Lookup, LookupDto> where Lookup : class 
    where LookupDto : class
{
    Task<Result<PagedResult<LookupDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result> CreateAsync(string name, CancellationToken cancellationToken);
    Task<Result> DeleteByNameAsync(string name, CancellationToken cancellationToken);
}