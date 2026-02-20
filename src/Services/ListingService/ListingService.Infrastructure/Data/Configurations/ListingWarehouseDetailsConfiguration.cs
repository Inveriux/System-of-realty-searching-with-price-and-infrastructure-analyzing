using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class WarehouseDetailsConfiguration : IEntityTypeConfiguration<WarehouseDetails>
{
    private const string TableName = "warehouse_details";

    public void Configure(EntityTypeBuilder<WarehouseDetails> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.ListingId);

        builder.Property(x => x.ListingId)
            .IsRequired()
            .ValueGeneratedNever();
        builder.HasOne(x => x.Listing)
            .WithOne()
            .HasForeignKey<WarehouseDetails>(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ClassTypeId)
            .IsRequired();

        builder.Property(x => x.HeatingTypeId)
            .IsRequired();

        builder.HasOne(x => x.ClassType)
            .WithMany()
            .HasForeignKey(x => x.ClassTypeId);

        builder.HasOne(x => x.HeatingType)
            .WithMany()
            .HasForeignKey(x => x.HeatingTypeId);
    }
}
