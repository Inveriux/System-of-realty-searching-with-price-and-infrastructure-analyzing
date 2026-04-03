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

    public async Task<PagedResult<Listing>> GetAllDetailedAsync(
        PagedRequest pageRequest,
        bool isTracking = false,
        CancellationToken cancellationToken = default)
    {
        var pagedListings = await context.Listings
            .IsTracking(isTracking)
            .Include(x => x.Media)
            .Include(x => x.PriceHistory)
            .Include(x => x.ListingType)
            .Include(x => x.BuildingType)
            .ToPagedResultAsync(pageRequest.PageNumber, pageRequest.PageSize, cancellationToken);

        if (pagedListings.Items.Count == 0)
        {
            return pagedListings;
        }

        var listingsById = pagedListings.Items.ToDictionary(x => x.Id);
        var listingIds = listingsById.Keys.ToArray();

        await LoadApartmentDetailsAsync(listingIds, listingsById, isTracking, cancellationToken);
        await LoadHouseDetailsAsync(listingIds, listingsById, isTracking, cancellationToken);
        await LoadLandDetailsAsync(listingIds, listingsById, isTracking, cancellationToken);
        await LoadOfficeDetailsAsync(listingIds, listingsById, isTracking, cancellationToken);
        await LoadWarehouseDetailsAsync(listingIds, listingsById, isTracking, cancellationToken);

        return pagedListings;
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
            throw new ArgumentException("Идентификатор не должен быть пустым.");

        if (!CheckDetailsType(listing.ListingDetailsType, details))
            throw new ArgumentException("Тип деталей объявления не соответствует ожидаемому.");

        var listingEntity = await context.AddAsync(listing, cancellationToken);
        details.ListingId = listingEntity.Entity.Id;
        
        await SaveDetails(listing.ListingDetailsType, details);
        await SavePriceHistoryAsync(listing, cancellationToken);
    }

    public async Task ReplaceDetailsAsync(
        Guid listingId,
        string listingDetailsType,
        IListingDetails details,
        CancellationToken cancellationToken)
    {
        if (!CheckDetailsType(listingDetailsType, details))
            throw new ArgumentException("Тип деталей объявления не соответствует ожидаемому.");

        await RemoveDetailsAsync<ApartmentDetails>(listingId, cancellationToken);
        await RemoveDetailsAsync<HouseDetails>(listingId, cancellationToken);
        await RemoveDetailsAsync<LandDetails>(listingId, cancellationToken);
        await RemoveDetailsAsync<OfficeDetails>(listingId, cancellationToken);
        await RemoveDetailsAsync<WarehouseDetails>(listingId, cancellationToken);

        details.ListingId = listingId;
        await SaveDetails(listingDetailsType, details);
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

    private async Task RemoveDetailsAsync<TDetails>(Guid listingId, CancellationToken cancellationToken)
        where TDetails : class, IListingDetails
    {
        var entity = await context.Set<TDetails>().FirstOrDefaultAsync(x => x.ListingId == listingId, cancellationToken);
        if (entity is not null)
        {
            context.Set<TDetails>().Remove(entity);
        }
    }

    public async Task<ListingPriceHistory?> GetLastPriceAsync(Guid listingId, CancellationToken cancellationToken)
    {
        return await context.ListingPriceHistories
            .AsNoTracking()
            .Where(h => h.ListingId == listingId)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ListingPriceHistory>> GetPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken)
        => await context.ListingPriceHistories
            .AsNoTracking()
            .Where(x => x.ListingId == listingId)
            .OrderBy(x => x.ChangedAt)
            .ToListAsync(cancellationToken);

    public async Task<ListingPriceHistory?> GetPriceHistoryEntryAsync(
        Guid listingId,
        Guid historyEntryId,
        bool isTracking,
        CancellationToken cancellationToken)
        => await context.ListingPriceHistories
            .IsTracking(isTracking)
            .FirstOrDefaultAsync(
                x => x.ListingId == listingId && x.Id == historyEntryId,
                cancellationToken);

    public async Task AddPriceHistoryEntryAsync(ListingPriceHistory entry, CancellationToken cancellationToken)
        => await context.ListingPriceHistories.AddAsync(entry, cancellationToken);

    public void RemovePriceHistoryEntry(ListingPriceHistory entry)
        => context.ListingPriceHistories.Remove(entry);

    public async Task SyncCurrentPriceWithLatestHistoryAsync(Guid listingId, CancellationToken cancellationToken)
    {
        var latest = await context.ListingPriceHistories
            .Where(x => x.ListingId == listingId)
            .OrderByDescending(x => x.ChangedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latest is null)
        {
            return;
        }

        var listing = await context.Listings.FirstOrDefaultAsync(x => x.Id == listingId, cancellationToken)
            ?? throw new ArgumentException($"There is no listing with id: {listingId}");

        listing.ChangePrice(latest);
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
        if(listing.PriceAmountByn is null)
            return;

        var priceHistory = new ListingPriceHistory()
        {
            PriceCurrency = listing.PriceCurrency,
            PriceAmount = listing.PriceAmount,
            PriceAmountByn = listing.PriceAmountByn,
            PriceAmountUsd = listing.PriceAmountUsd,
            ChangedAt = DateTimeOffset.UtcNow,
            ListingId = listing.Id,
            Comment = null
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

    private async Task LoadApartmentDetailsAsync(
        IReadOnlyCollection<Guid> listingIds,
        IReadOnlyDictionary<Guid, Listing> listingsById,
        bool isTracking,
        CancellationToken cancellationToken)
    {
        var details = await context.Set<ApartmentDetails>()
            .IsTracking(isTracking)
            .Include(x => x.BathroomType)
            .Include(x => x.RenovationType)
            .Include(x => x.BuildingMaterial)
            .Where(x => listingIds.Contains(x.ListingId))
            .ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            if (listingsById.TryGetValue(detail.ListingId, out var listing))
            {
                listing.ListingDetails = detail;
            }
        }
    }

    private async Task LoadHouseDetailsAsync(
        IReadOnlyCollection<Guid> listingIds,
        IReadOnlyDictionary<Guid, Listing> listingsById,
        bool isTracking,
        CancellationToken cancellationToken)
    {
        var details = await context.Set<HouseDetails>()
            .IsTracking(isTracking)
            .Include(x => x.WallMaterial)
            .Include(x => x.HeatingType)
            .Include(x => x.SewerType)
            .Include(x => x.GasType)
            .Include(x => x.RoofMaterial)
            .Include(x => x.WaterSupplyType)
            .Where(x => listingIds.Contains(x.ListingId))
            .ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            if (listingsById.TryGetValue(detail.ListingId, out var listing))
            {
                listing.ListingDetails = detail;
            }
        }
    }

    private async Task LoadLandDetailsAsync(
        IReadOnlyCollection<Guid> listingIds,
        IReadOnlyDictionary<Guid, Listing> listingsById,
        bool isTracking,
        CancellationToken cancellationToken)
    {
        var details = await context.Set<LandDetails>()
            .IsTracking(isTracking)
            .Include(x => x.LandCategory)
            .Include(x => x.RoadAccessType)
            .Where(x => listingIds.Contains(x.ListingId))
            .ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            if (listingsById.TryGetValue(detail.ListingId, out var listing))
            {
                listing.ListingDetails = detail;
            }
        }
    }

    private async Task LoadOfficeDetailsAsync(
        IReadOnlyCollection<Guid> listingIds,
        IReadOnlyDictionary<Guid, Listing> listingsById,
        bool isTracking,
        CancellationToken cancellationToken)
    {
        var details = await context.Set<OfficeDetails>()
            .IsTracking(isTracking)
            .Include(x => x.OfficeClass)
            .Include(x => x.LayoutType)
            .Include(x => x.RenovationType)
            .Include(x => x.ParkingType)
            .Where(x => listingIds.Contains(x.ListingId))
            .ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            if (listingsById.TryGetValue(detail.ListingId, out var listing))
            {
                listing.ListingDetails = detail;
            }
        }
    }

    private async Task LoadWarehouseDetailsAsync(
        IReadOnlyCollection<Guid> listingIds,
        IReadOnlyDictionary<Guid, Listing> listingsById,
        bool isTracking,
        CancellationToken cancellationToken)
    {
        var details = await context.Set<WarehouseDetails>()
            .IsTracking(isTracking)
            .Include(x => x.ClassType)
            .Include(x => x.HeatingType)
            .Where(x => listingIds.Contains(x.ListingId))
            .ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            if (listingsById.TryGetValue(detail.ListingId, out var listing))
            {
                listing.ListingDetails = detail;
            }
        }
    }
}
