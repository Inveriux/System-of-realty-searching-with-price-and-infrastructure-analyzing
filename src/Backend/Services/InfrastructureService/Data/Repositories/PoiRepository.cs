using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Repositories;

public sealed class PoiRepository(ApplicationDbContext context) : IPoiRepository
{
    public async Task<IReadOnlyCollection<PoiCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        => await context.PoiCategories
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<PoiCategory?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken)
        => context.PoiCategories.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task<Poi> UpsertAsync(Poi poi, CancellationToken cancellationToken)
    {
        var existing = await context.Pois.FirstOrDefaultAsync(
            x => x.CategoryId == poi.CategoryId && x.ExternalId == poi.ExternalId && x.ExternalId != null,
            cancellationToken);

        if (existing is not null)
        {
            existing.Name = poi.Name;
            existing.AddressText = poi.AddressText;
            existing.Latitude = poi.Latitude;
            existing.Longitude = poi.Longitude;
            existing.SubcategoryCode = poi.SubcategoryCode;
            existing.Source = poi.Source;
            return existing;
        }

        await context.Pois.AddAsync(poi, cancellationToken);
        return poi;
    }

    public Task AddCategoryAsync(PoiCategory category, CancellationToken cancellationToken)
        => context.PoiCategories.AddAsync(category, cancellationToken).AsTask();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
