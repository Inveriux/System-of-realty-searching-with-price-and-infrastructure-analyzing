using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Repositories;

public sealed class InfrastructureRepository(ApplicationDbContext context) : IInfrastructureRepository
{
    public Task<LocationPoiSnapshot?> GetLatestSnapshotAsync(Guid locationId, CancellationToken cancellationToken)
        => context.LocationPoiSnapshots
            .Include(x => x.Items)
            .ThenInclude(x => x.Poi)
            .ThenInclude(x => x!.Category)
            .Include(x => x.InfrastructureScore)
            .Where(x => x.LocationId == locationId)
            .OrderByDescending(x => x.RefreshedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<InfrastructureScore?> GetScoreAsync(Guid locationId, CancellationToken cancellationToken)
        => context.InfrastructureScores.FirstOrDefaultAsync(x => x.LocationId == locationId, cancellationToken);

    public async Task<IReadOnlyCollection<LocationPoiSnapshotItem>> GetSnapshotItemsAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var snapshot = await GetLatestSnapshotAsync(locationId, cancellationToken);
        return snapshot?.Items.OrderBy(x => x.DistanceMeters).ToList() ?? [];
    }

    public Task AddSnapshotAsync(LocationPoiSnapshot snapshot, CancellationToken cancellationToken)
        => context.LocationPoiSnapshots.AddAsync(snapshot, cancellationToken).AsTask();

    public async Task UpsertScoreAsync(InfrastructureScore score, CancellationToken cancellationToken)
    {
        var existing = await context.InfrastructureScores.FirstOrDefaultAsync(x => x.LocationId == score.LocationId, cancellationToken);
        if (existing is null)
        {
            await context.InfrastructureScores.AddAsync(score, cancellationToken);
            return;
        }

        existing.SnapshotId = score.SnapshotId;
        existing.TotalScore = score.TotalScore;
        existing.EducationScore = score.EducationScore;
        existing.HealthcareScore = score.HealthcareScore;
        existing.TransportScore = score.TransportScore;
        existing.CommerceScore = score.CommerceScore;
        existing.LeisureScore = score.LeisureScore;
        existing.PoiCount = score.PoiCount;
        existing.CalculatedAtUtc = score.CalculatedAtUtc;
        existing.SearchFeaturesJson = score.SearchFeaturesJson;
        existing.DetailedSummaryJson = score.DetailedSummaryJson;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
