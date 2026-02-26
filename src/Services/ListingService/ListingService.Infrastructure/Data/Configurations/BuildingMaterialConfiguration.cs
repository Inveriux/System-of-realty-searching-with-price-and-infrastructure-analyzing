using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class BuildingMaterialConfiguration : IEntityTypeConfiguration<BuildingMaterial>
{
    private const string TableName = "building_material";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<BuildingMaterial> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(StringMaxLength)
            .IsRequired();
    }
}
