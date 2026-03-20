using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureService.Data.Configurations;

internal sealed class PoiConfiguration : IEntityTypeConfiguration<Poi>
{
    public void Configure(EntityTypeBuilder<Poi> builder)
    {
        builder.ToTable("poi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExternalId).HasMaxLength(200);
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.AddressText).HasMaxLength(500);
        builder.Property(x => x.Latitude).HasPrecision(10, 6).IsRequired();
        builder.Property(x => x.Longitude).HasPrecision(10, 6).IsRequired();

        builder.HasIndex(x => new { x.CategoryId, x.ExternalId });

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Pois)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
