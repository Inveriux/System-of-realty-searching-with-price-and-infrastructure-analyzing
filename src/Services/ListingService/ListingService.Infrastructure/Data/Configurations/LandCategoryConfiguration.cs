using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class LandCategoryConfiguration : IEntityTypeConfiguration<LandCategory>
{
    private const string TableName = "land_category";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<LandCategory> builder)
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
