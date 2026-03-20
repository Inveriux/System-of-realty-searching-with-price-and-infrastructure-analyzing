using BasicBuildingBlocks.Collections;
using Microsoft.EntityFrameworkCore;
using SearchService.Data.Repositories.Abstractions;
using SearchService.Models;

namespace SearchService.Data.Repositories;

public sealed class SearchListingRepository(ApplicationDbContext context) : ISearchListingRepository
{
    public Task<SearchListing?> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken)
        => context.SearchListings.FirstOrDefaultAsync(x => x.ListingId == listingId, cancellationToken);

    public async Task<IReadOnlyCollection<SearchListing>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        => await context.SearchListings
            .Where(x => x.LocationId == locationId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<SearchListing>> SearchAsync(
        string? query,
        Guid? locationId,
        string? listingType,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var searchQuery = context.SearchListings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            searchQuery = searchQuery.Where(x =>
                EF.Functions.ILike(x.Title, pattern) ||
                (x.Description != null && EF.Functions.ILike(x.Description, pattern)) ||
                (x.AddressText != null && EF.Functions.ILike(x.AddressText, pattern)));
        }

        if (locationId.HasValue)
            searchQuery = searchQuery.Where(x => x.LocationId == locationId.Value);

        if (!string.IsNullOrWhiteSpace(listingType))
            searchQuery = searchQuery.Where(x => x.ListingType == listingType);

        if (minPrice.HasValue)
            searchQuery = searchQuery.Where(x => x.PriceAmount >= minPrice.Value);

        if (maxPrice.HasValue)
            searchQuery = searchQuery.Where(x => x.PriceAmount <= maxPrice.Value);

        var totalCount = await searchQuery.LongCountAsync(cancellationToken);
        var items = await searchQuery
            .OrderByDescending(x => x.InfrastructureScore ?? 0m)
            .ThenByDescending(x => x.PublishedDate)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SearchListing>(items, page, pageSize, totalCount);
    }

    public async Task UpsertAsync(SearchListing listing, CancellationToken cancellationToken)
    {
        var existing = await context.SearchListings.FirstOrDefaultAsync(x => x.ListingId == listing.ListingId, cancellationToken);
        if (existing is null)
        {
            await context.SearchListings.AddAsync(listing, cancellationToken);
            return;
        }

        existing.Title = listing.Title;
        existing.Description = listing.Description;
        existing.AddressText = listing.AddressText;
        existing.LocationId = listing.LocationId;
        existing.Latitude = listing.Latitude;
        existing.Longitude = listing.Longitude;
        existing.ListingType = listing.ListingType;
        existing.BuildingType = listing.BuildingType;
        existing.PriceAmount = listing.PriceAmount;
        existing.PriceCurrency = listing.PriceCurrency;
        existing.AreaTotal = listing.AreaTotal;
        existing.MainMediaUrl = listing.MainMediaUrl;
        existing.PublishedDate = listing.PublishedDate;
        existing.InfrastructureScore = listing.InfrastructureScore;
        existing.SearchFeaturesJson = listing.SearchFeaturesJson;
        existing.UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Remove(SearchListing listing)
        => context.SearchListings.Remove(listing);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
