using ListingService.Application.Contracts.Cache;
using ListingService.Domain.Abstractions;
using AutoMapper;

namespace ListingService.Application.Common.Lookups.Converters;

public sealed class LookupByNameConverter<TLookupEntity>(ILookupCache cache)
    : ITypeConverter<string, TLookupEntity>
    where TLookupEntity : class, ILookupEntity
{
    public TLookupEntity Convert(string source, TLookupEntity destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new AutoMapperMappingException("Lookup name is empty.");

        var lookupName = typeof(TLookupEntity).Name;

        var entity = cache.TryGetByName<TLookupEntity>(lookupName, source);
        if (entity is not null)
            return entity;

        throw new AutoMapperMappingException(
            $"Lookup '{lookupName}' with name '{source}' not found in cache.");
    }
}

public sealed class LookupByNameOrNullConverter<TLookupEntity>(ILookupCache cache)
    : ITypeConverter<string?, TLookupEntity?>
    where TLookupEntity : class, ILookupEntity
{
    public TLookupEntity? Convert(string? source, TLookupEntity? destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            return null;

        var lookupName = typeof(TLookupEntity).Name;

        var entity = cache.TryGetByName<TLookupEntity>(lookupName, source);
        if (entity is not null)
            return entity;

        throw new AutoMapperMappingException(
            $"Lookup '{lookupName}' with name '{source}' not found in cache.");
    }
}