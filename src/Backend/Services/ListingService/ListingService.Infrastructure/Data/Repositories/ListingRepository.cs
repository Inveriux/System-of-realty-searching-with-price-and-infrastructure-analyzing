using ListingService.Application.Contracts.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BasicBuildingBlocks.Extensions.Repositories;
using ListingService.Domain.Abstractions;
using BasicBuildingBlocks.Collections;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Repositories;

public class ListingRepository(ApplicationDbContext context) : IListingRepository
{
    //TODO Add sorting
    public async Task<PagedResult<Listing>> GetAllAsync(
        PagedRequest pageRequest,
        bool isTracking = false,
        CancellationToken cancellationToken = default)
    {
        return await context.Listings
            .IsTracking(isTracking)
            .Include(x => x.ListingType)
            .Include(x => x.Media)
            .Include(x => x.BuildingType)
            .ToPagedResultAsync(pageRequest.PageNumber, pageRequest.PageSize, cancellationToken);
    }

    public async Task<Listing?> GetByIdAsync(Guid id, 
        bool isTracking = false,
        CancellationToken cancellationToken = default)
    {
        return await context.Listings
            .IsTracking(isTracking)
            .Include(x => x.ListingType)
            .Include(x => x.BuildingType)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Listing?> GetDetailedByIdAsync(
        Guid id,
        bool isTracking = false,
        CancellationToken cancellationToken = default)
    {
        var listing = await context.Listings
            .IsTracking(isTracking)
            .Include(x => x.Media)
            .Include(x => x.PriceHistory)
            .Include(x => x.ListingType)
            .Include(x => x.BuildingType)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (listing is null)
            return null;

        IListingDetails? details = listing.ListingDetailsType switch
        {
            ListingDetailsType.Apartment =>
                await context.Set<ApartmentDetails>()
                    .IsTracking(isTracking)
                    .FirstOrDefaultAsync(d => d.ListingId == id, cancellationToken),

            ListingDetailsType.House =>
                await context.Set<HouseDetails>()
                    .IsTracking(isTracking)
                    .FirstOrDefaultAsync(d => d.ListingId == id, cancellationToken),

            ListingDetailsType.Office =>
                await context.Set<OfficeDetails>()
                    .IsTracking(isTracking)
                    .FirstOrDefaultAsync(d => d.ListingId == id, cancellationToken),

            ListingDetailsType.Warehouse =>
                await context.Set<WarehouseDetails>()
                    .IsTracking(isTracking)
                    .FirstOrDefaultAsync(d => d.ListingId == id, cancellationToken),

            ListingDetailsType.Land =>
                await context.Set<LandDetails>()
                    .IsTracking(isTracking)
                    .FirstOrDefaultAsync(d => d.ListingId == id, cancellationToken),

            _ => null
        };

        listing.ListingDetails = details;

        return listing;
    }

    public async Task AddAsync(Listing listing,
        CancellationToken cancellationToken)
    {
        if(listing.ListingDetails is null)
            throw new NullReferenceException("Listing should has details for him type.");
        var details = listing.ListingDetails;
        
        if(Guid.Empty == listing.Id)
            throw new ArgumentException("Id must be not empty.");

        if (!CheckDetailsType(listing.ListingDetailsType, details))
            throw new ArgumentException("Details type mismatch.");

        var listingEntity = await context.AddAsync(listing, cancellationToken);
        details.ListingId = listingEntity.Entity.Id;
        
        await SaveDetails(listing.ListingDetailsType, details);
        await SavePriceHistoryAsync(listing, cancellationToken);
    }

    private bool CheckDetailsType(string type, IListingDetails details)
    {
        if(type == ListingDetailsType.Apartment
            && details is ApartmentDetails)
            return true;
        
        if(type == ListingDetailsType.House
            && details is HouseDetails)
            return true;

        if(type == ListingDetailsType.Office
            && details is OfficeDetails)
            return true;

        if(type == ListingDetailsType.Warehouse
            && details is WarehouseDetails)
            return true;

        if(type == ListingDetailsType.Land
            && details is LandDetails)
            return true;   

        return false;     
    }

    private async Task<EntityEntry?> SaveDetails(string type, IListingDetails details)
    {
        if(type == ListingDetailsType.Apartment
            && details is ApartmentDetails apartmentDetails)
            return await context.Set<ApartmentDetails>()
                .AddAsync(apartmentDetails);
        
        if(type == ListingDetailsType.House
            && details is HouseDetails houseDetails)
            return await context.Set<HouseDetails>()
                .AddAsync(houseDetails);

        if(type == ListingDetailsType.Office
            && details is OfficeDetails officeDetails)
            return await context.Set<OfficeDetails>()
                .AddAsync(officeDetails);

        if(type == ListingDetailsType.Warehouse
            && details is WarehouseDetails warehouseDetails)
            return await context.Set<WarehouseDetails>()
                .AddAsync(warehouseDetails);

        if(type == ListingDetailsType.Land
            && details is LandDetails landDetails)
            return await context.Set<LandDetails>()
                .AddAsync(landDetails);  

        return null;
    }

    public async Task<ListingPriceHistory?> GetLastPriceAsync(Guid listingId, CancellationToken cancellationToken)
    {
        return await context.ListingPriceHistories
            .AsNoTracking()
            .Where(h => h.ListingId == listingId)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task ChangePriceHistoryAsync(Guid listingId, 
        ListingPriceHistory newPriceHistory, 
        CancellationToken cancellationToken)
    {
        var listing = await GetByIdAsync(listingId, true, cancellationToken) 
            ?? throw new ArgumentException($"There is no listing with id: {listingId}");

        var lastPrice = await GetLastPriceAsync(listingId, cancellationToken);
        if(lastPrice is not null
            && lastPrice.IsPriceEqual(newPriceHistory))
            return;

        listing.ChangePrice(newPriceHistory);
        await SavePriceHistoryAsync(listing, cancellationToken);
    }

    private async Task SavePriceHistoryAsync(Listing listing, CancellationToken cancellationToken)
    {
        if(listing.PriceAmount is null)
            return;

        var priceHistory = new ListingPriceHistory()
        {
            PriceCurrency = listing.PriceCurrency,
            PriceAmount = listing.PriceAmount,
            ChangedAt = DateTimeOffset.UtcNow,
            ListingId = listing.Id
        };

        await context.AddAsync(priceHistory, cancellationToken);
    }

    public Task UpdateAsync(Listing listing, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        => DeleteInternalAsync(id, cancellationToken);

    private async Task DeleteInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var listing = await context.Listings
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new ArgumentException($"There is no listing with id: {id}");

        context.Listings.Remove(listing);
    }
}
