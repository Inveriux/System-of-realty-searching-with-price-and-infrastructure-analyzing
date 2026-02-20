using ListingService.Application.Contracts.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ListingService.Infrastructure.Data.Repositories;

public class RepositoryManager(ApplicationDbContext context, IServiceProvider serviceProvider) 
    : IRepositoryManager
{
    private readonly Lazy<IListingRepository> _listingRepository = 
        new(serviceProvider.GetRequiredService<IListingRepository>());
    public IListingRepository Listing => _listingRepository.Value;

    public ILookupRepository<LookupTable> GetLookup<LookupTable>() where LookupTable : class
        => serviceProvider.GetRequiredService<ILookupRepository<LookupTable>>();

    public bool SaveChanges() => context.SaveChanges() > 0;

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
        => await context.SaveChangesAsync(cancellationToken) > 0;
}