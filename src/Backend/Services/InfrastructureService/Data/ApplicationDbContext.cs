using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<PoiCategory> PoiCategories => Set<PoiCategory>();
    public DbSet<Poi> Pois => Set<Poi>();
    public DbSet<LocationPoiSnapshot> LocationPoiSnapshots => Set<LocationPoiSnapshot>();
    public DbSet<LocationPoiSnapshotItem> LocationPoiSnapshotItems => Set<LocationPoiSnapshotItem>();
    public DbSet<InfrastructureScore> InfrastructureScores => Set<InfrastructureScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
