using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureService.Data.Configurations;

internal sealed class PoiCategoryConfiguration : IEntityTypeConfiguration<PoiCategory>
{
    public void Configure(EntityTypeBuilder<PoiCategory> builder)
    {
        builder.ToTable("poi_category");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SearchText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
