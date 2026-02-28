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
            [nameof(BathroomType)] = (LoadAsync<BathroomType>),
            [nameof(BuildingMaterial)] = (LoadAsync<BuildingMaterial>),
            [nameof(BuildingType)] = (LoadAsync<BuildingType>),
            [nameof(ClassType)] = (LoadAsync<ClassType>),
            [nameof(GasType)] = (LoadAsync<GasType>),
            [nameof(HeatingType)] = (LoadAsync<HeatingType>),
            [nameof(LandCategory)] = (LoadAsync<LandCategory>),
            [nameof(LayoutType)] = (LoadAsync<LayoutType>),
            [nameof(ListingType)] = (LoadAsync<ListingType>),
            [nameof(OfficeClass)] = (LoadAsync<OfficeClass>),
            [nameof(RenovationType)] = (LoadAsync<RenovationType>),
            [nameof(RoadAccessType)] = (LoadAsync<RoadAccessType>),
            [nameof(SewerType)] = (LoadAsync<SewerType>),
            [nameof(WallMaterial)] = (LoadAsync<WallMaterial>),
            [nameof(WaterSupplyType)] = (LoadAsync<WaterSupplyType>)
        };

        _loaders = new(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(BathroomType)] = (Load<BathroomType>),
            [nameof(BuildingMaterial)] = (Load<BuildingMaterial>),
            [nameof(BuildingType)] = (Load<BuildingType>),
            [nameof(ClassType)] = (Load<ClassType>),
            [nameof(GasType)] = (Load<GasType>),
            [nameof(HeatingType)] = (Load<HeatingType>),
            [nameof(LandCategory)] = (Load<LandCategory>),
            [nameof(LayoutType)] = (Load<LayoutType>),
            [nameof(ListingType)] = (Load<ListingType>),
            [nameof(OfficeClass)] = (Load<OfficeClass>),
            [nameof(RenovationType)] = (Load<RenovationType>),
            [nameof(RoadAccessType)] = (Load<RoadAccessType>),
            [nameof(SewerType)] = (Load<SewerType>),
            [nameof(WallMaterial)] = (Load<WallMaterial>),
            [nameof(WaterSupplyType)] = (Load<WaterSupplyType>)
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