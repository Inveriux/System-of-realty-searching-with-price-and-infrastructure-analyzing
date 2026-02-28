using ListingService.Application.Contracts.Cache;
using Microsoft.Extensions.Caching.Memory;
using ListingService.Domain.Abstractions;
using System.Collections.Concurrent; 
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Cache;

public class LookupCache : ILookupCache
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];
    private const int CacheTimeMinutes = 30;
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(CacheTimeMinutes);
    private string NormalizeKey(string lookupName) 
        => $"{lookupName.ToLowerInvariant()}";

    private readonly Dictionary<string, Func<CancellationToken, Task<IEnumerable<ILookupEntity>>>> _asyncLoaders;
    private readonly Dictionary<string, Func<IEnumerable<ILookupEntity>>> _loaders;
    private readonly IRepositoryManager _manager;
    private readonly IMemoryCache _cache;


    public LookupCache(IRepositoryManager manager, IMemoryCache cache)
    {
        _manager = manager;
        _cache = cache;

        _asyncLoaders = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bathroomtype"] = (LoadAsync<BathroomType>),
            ["buildingmaterial"] = (LoadAsync<BuildingMaterial>),
            ["buildingtype"] = (LoadAsync<BuildingType>),
            ["classtype"] = (LoadAsync<ClassType>),
            ["gastype"] = (LoadAsync<GasType>),
            ["heatingtype"] = (LoadAsync<HeatingType>),
            ["landcategory"] = (LoadAsync<LandCategory>),
            ["layouttype"] = (LoadAsync<LayoutType>),
            ["listingtype"] = (LoadAsync<ListingType>),
            ["officeclass"] = (LoadAsync<OfficeClass>),
            ["renovationtype"] = (LoadAsync<RenovationType>),
            ["roadaccesstype"] = (LoadAsync<RoadAccessType>),
            ["sewertype"] = (LoadAsync<SewerType>),
            ["wallmaterial"] = (LoadAsync<WallMaterial>),
            ["watersupplytype"] = (LoadAsync<WaterSupplyType>)
        };

        _loaders = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bathroomtype"] = (Load<BathroomType>),
            ["buildingmaterial"] = (Load<BuildingMaterial>),
            ["buildingtype"] = (Load<BuildingType>),
            ["classtype"] = (Load<ClassType>),
            ["gastype"] = (Load<GasType>),
            ["heatingtype"] = (Load<HeatingType>),
            ["landcategory"] = (Load<LandCategory>),
            ["layouttype"] = (Load<LayoutType>),
            ["listingtype"] = (Load<ListingType>),
            ["officeclass"] = (Load<OfficeClass>),
            ["renovationtype"] = (Load<RenovationType>),
            ["roadaccesstype"] = (Load<RoadAccessType>),
            ["sewertype"] = (Load<SewerType>),
            ["wallmaterial"] = (Load<WallMaterial>),
            ["watersupplytype"] = (Load<WaterSupplyType>)
        };
    }

    public async Task<Guid> ResolveIdAsync<TLookupEntity>(
        string lookupName,
        string code,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity
    {
        var key = NormalizeKey(lookupName);
        var lookupData = await GetOrAddAsync<TLookupEntity>(key, cancellationToken);

        var entity = lookupData.FirstOrDefault(e =>
            string.Equals(e.Name, code, StringComparison.OrdinalIgnoreCase));

        return entity?.Id ?? Guid.Empty;
    }

    public async Task<TLookupEntity?> TryGetByNameAsync<TLookupEntity>(
        string lookupName,
        string name,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity
    {
        var key = NormalizeKey(lookupName);
        var lookupData = await GetOrAddAsync<TLookupEntity>(key, cancellationToken);

        return lookupData.FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public TLookupEntity? TryGetByName<TLookupEntity>(string lookupName, string name)
        where TLookupEntity : class, ILookupEntity
    {
        var key = NormalizeKey(lookupName);
        var lookupData = GetOrAdd<TLookupEntity>(key);

        return lookupData.FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<TLookupEntity>> GetAllAsync<TLookupEntity>(
        string lookupName,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity
    {
        var key = NormalizeKey(lookupName);
        var lookupData = await GetOrAddAsync<TLookupEntity>(key, cancellationToken);
        return lookupData.ToList();
    }

    private async Task<IEnumerable<TLookupEntity>> GetOrAddAsync<TLookupEntity>(
        string key,
        CancellationToken cancellationToken = default)
        where TLookupEntity : class, ILookupEntity
    {
        if (_cache.TryGetValue<IEnumerable<TLookupEntity>>(key, out var cached) && cached is not null)
            return cached;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(key, out cached) && cached is not null)
                return cached;

            var lookupData = await _asyncLoaders[key](cancellationToken);
            var typed = lookupData.Cast<TLookupEntity>().ToArray(); 

            _cache.Set(key, typed, _cacheDuration);
            return typed;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private IEnumerable<TLookupEntity> GetOrAdd<TLookupEntity>(string key)
        where TLookupEntity : class, ILookupEntity
    {
        if (_cache.TryGetValue<IEnumerable<TLookupEntity>>(key, out var cached) && cached is not null)
            return cached;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        semaphore.Wait();

        try
        {
            if (_cache.TryGetValue<IEnumerable<TLookupEntity>>(key, out cached) && cached is not null)
                return cached;

            if (!_loaders.TryGetValue(key, out var loader))
                return Array.Empty<TLookupEntity>();

            var lookupData = loader(); // IEnumerable<ILookupEntity>
            var typed = lookupData.Cast<TLookupEntity>().ToArray();

            _cache.Set(key, typed, _cacheDuration);
            return typed;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Invalidate(string lookupName)
    {
        _cache.Remove(NormalizeKey(lookupName));
    }

    private async Task<IEnumerable<ILookupEntity>> LoadAsync<TLookup>(
        CancellationToken cancellationToken = default) where TLookup : class, ILookupEntity
    {
        return await _manager
            .GetLookup<TLookup>()
            .GetAllAsync<string>(cancellationToken);
    }

    private IEnumerable<ILookupEntity> Load<TLookup>() where TLookup : class, ILookupEntity
    {
        return _manager
            .GetLookup<TLookup>()
            .GetAll<string>();
    }
}