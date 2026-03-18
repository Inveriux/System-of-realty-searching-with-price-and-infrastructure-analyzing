using ListingService.Application.DTOs.Lookups.Abstractions;
using ListingService.Domain.Abstractions;
namespace ListingService.Application.Common.Lookups.Abstractions;

public interface ILookupHandler<TLookup, TLookupDto> 
    where TLookup : class, ILookupEntity
    where TLookupDto : class, ILookupDto
{
    Task<Result<List<TLookupDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result> CreateAsync(TLookupDto lookupDto, CancellationToken cancellationToken);
    Task<Result> DeleteByNameAsync(string name, CancellationToken cancellationToken);
}
