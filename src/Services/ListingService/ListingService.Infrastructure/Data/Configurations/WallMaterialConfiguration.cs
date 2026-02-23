using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class WallMaterialConfiguration : IEntityTypeConfiguration<WallMaterial>
{
    private const string TableName = "wall_material";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<WallMaterial> builder)
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
