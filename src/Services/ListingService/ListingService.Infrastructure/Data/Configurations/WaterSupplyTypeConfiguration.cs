using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class WaterSupplyTypeConfiguration : IEntityTypeConfiguration<WaterSupplyType>
{
    private const string TableName = "water_supply_type";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<WaterSupplyType> builder)
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
