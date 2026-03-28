using ListingService.Application.Contracts.Cache;
using ListingService.Domain.Abstractions;
using FluentValidation;

namespace ListingService.Application.Validators.Listings.Extensions;

public static class LookupValidationExtensions
{
    public static IRuleBuilder<T, string?> MustExistInLookup<T, TLookup>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ILookupCache lookupCache,
        string lookupName,
        string? propertyName = null)
        where TLookup : class, ILookupEntity
    {
        return ruleBuilder.MustAsync(async (value, cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(value)) return true;

            try
            {
                var id = await lookupCache.ResolveIdAsync<TLookup>(lookupName, value, cancellationToken);
                return id != Guid.Empty;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }).WithMessage(_ =>
            string.IsNullOrWhiteSpace(propertyName)
                ? "'{PropertyName}' has unknown value."
                : $"'{propertyName}' has unknown value.");
    }

    public static IRuleBuilder<T, string?> MustExistInLookupRequired<T, TLookup>(
        this IRuleBuilder<T, string?> ruleBuilder,
        ILookupCache lookupCache,
        string lookupName)
        where TLookup : class, ILookupEntity
    {
        return ruleBuilder
            .NotEmpty()
            .MustExistInLookup<T, TLookup>(lookupCache, lookupName);
    }
}
