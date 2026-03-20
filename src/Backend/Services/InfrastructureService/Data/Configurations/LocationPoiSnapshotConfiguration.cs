using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureService.Data.Configurations;

internal sealed class LocationPoiSnapshotConfiguration : IEntityTypeConfiguration<LocationPoiSnapshot>
{
    public void Configure(EntityTypeBuilder<LocationPoiSnapshot> builder)
    {
        builder.ToTable("location_poi_snapshot");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SearchRadiusMeters).IsRequired();

        builder.HasOne(x => x.Location)
            .WithMany(x => x.Snapshots)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
