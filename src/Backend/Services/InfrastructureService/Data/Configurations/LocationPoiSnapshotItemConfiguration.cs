using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureService.Data.Configurations;

internal sealed class LocationPoiSnapshotItemConfiguration : IEntityTypeConfiguration<LocationPoiSnapshotItem>
{
    public void Configure(EntityTypeBuilder<LocationPoiSnapshotItem> builder)
    {
        builder.ToTable("location_poi_snapshot_item");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DistanceMeters)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasOne(x => x.Snapshot)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Poi)
            .WithMany()
            .HasForeignKey(x => x.PoiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
