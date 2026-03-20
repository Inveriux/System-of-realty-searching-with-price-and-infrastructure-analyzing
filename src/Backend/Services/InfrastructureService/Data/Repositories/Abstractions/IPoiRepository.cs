using InfrastructureService.Models;

namespace InfrastructureService.Data.Repositories.Abstractions;

public interface IPoiRepository
{
    Task<IReadOnlyCollection<PoiCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<PoiCategory?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken);
    Task<Poi> UpsertAsync(Poi poi, CancellationToken cancellationToken);
    Task AddCategoryAsync(PoiCategory category, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
