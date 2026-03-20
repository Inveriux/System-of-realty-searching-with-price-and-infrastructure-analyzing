using InfrastructureService.Models;

namespace InfrastructureService.Data.Repositories.Abstractions;

public interface IInfrastructureRepository
{
    Task<LocationPoiSnapshot?> GetLatestSnapshotAsync(Guid locationId, CancellationToken cancellationToken);
    Task<InfrastructureScore?> GetScoreAsync(Guid locationId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LocationPoiSnapshotItem>> GetSnapshotItemsAsync(Guid locationId, CancellationToken cancellationToken);
    Task AddSnapshotAsync(LocationPoiSnapshot snapshot, CancellationToken cancellationToken);
    Task UpsertScoreAsync(InfrastructureScore score, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
