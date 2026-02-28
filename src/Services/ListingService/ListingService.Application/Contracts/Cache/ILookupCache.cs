using ListingService.Domain.Abstractions;

namespace ListingService.Application.Contracts.Cache;

public interface ILookupCache
{
    Task<Guid> ResolveIdAsync<TLookupEntity>(
        string lookupName,
        string code,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity;

    Task<TLookupEntity?> TryGetByNameAsync<TLookupEntity>(
        string lookupName,
        string name,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity;

    TLookupEntity? TryGetByName<TLookupEntity>(
        string lookupName,
        string name)
        where TLookupEntity : class, ILookupEntity;

    Task<List<TLookupEntity>> GetAllAsync<TLookupEntity>(
        string lookupName,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity;

    void Invalidate(string lookupName);
}