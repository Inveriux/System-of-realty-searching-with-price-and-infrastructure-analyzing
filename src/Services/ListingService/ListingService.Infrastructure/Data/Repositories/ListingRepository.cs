using ListingService.Application.Contracts.Repositories;
using BasicBuildingBlocks.Extensions.Repositories;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;

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
            .ToPagedResultAsync(pageRequest.PageNumber, pageRequest.PageSize, cancellationToken);
    }

    public Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Listing listing, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Listing listing, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public bool SaveChanges()
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}