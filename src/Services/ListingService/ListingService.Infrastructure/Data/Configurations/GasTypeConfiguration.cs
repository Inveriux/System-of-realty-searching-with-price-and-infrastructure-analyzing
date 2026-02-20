using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class GasTypeConfiguration : IEntityTypeConfiguration<GasType>
{
    private const string TableName = "gas_type";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<GasType> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.TypeName)
            .HasMaxLength(StringMaxLength)
            .IsRequired();
    }
}
