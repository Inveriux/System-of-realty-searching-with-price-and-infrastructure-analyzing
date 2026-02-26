using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingOfficeDetailsConfiguration : IEntityTypeConfiguration<OfficeDetails>
{
    private const string TableName = "office_details";

    public void Configure(EntityTypeBuilder<OfficeDetails> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.ListingId);

        builder.Property(x => x.ListingId)
            .IsRequired()
            .ValueGeneratedNever();
        builder.HasOne(x => x.Listing)
            .WithOne()
            .HasForeignKey<OfficeDetails>(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.OfficeClassId)
            .IsRequired();

        builder.Property(x => x.LayoutTypeId)
            .IsRequired();

        builder.Property(x => x.RenovationTypeId)
            .IsRequired();

        builder.Property(x => x.ParkingTypeId)
            .IsRequired();

        builder.HasOne(x => x.OfficeClass)
            .WithMany()
            .HasForeignKey(x => x.OfficeClassId);

        builder.HasOne(x => x.LayoutType)
            .WithMany()
            .HasForeignKey(x => x.LayoutTypeId);

        builder.HasOne(x => x.RenovationType)
            .WithMany()
            .HasForeignKey(x => x.RenovationTypeId);

        builder.HasOne(x => x.ParkingType)
            .WithMany()
            .HasForeignKey(x => x.ParkingTypeId);
    }
}
