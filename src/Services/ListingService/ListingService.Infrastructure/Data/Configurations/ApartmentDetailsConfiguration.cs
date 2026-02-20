using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ApartmentDetailsConfiguration : IEntityTypeConfiguration<ApartmentDetails>
{
    private const string TableName = "apartment_details";

    public void Configure(EntityTypeBuilder<ApartmentDetails> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.ListingId);

        builder.Property(x => x.ListingId)
            .IsRequired()
            .ValueGeneratedNever();
            
        builder.HasOne(x => x.Listing)
            .WithOne()
            .HasForeignKey<ApartmentDetails>(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.BathroomTypeId)
            .IsRequired();

        builder.Property(x => x.RenovationTypeId)
            .IsRequired();

        builder.Property(x => x.BuildingMaterialId)
            .IsRequired();

        builder.HasOne(x => x.BathroomType)
            .WithMany()
            .HasForeignKey(x => x.BathroomTypeId);

        builder.HasOne(x => x.RenovationType)
            .WithMany()
            .HasForeignKey(x => x.RenovationTypeId);

        builder.HasOne(x => x.BuildingMaterial)
            .WithMany()
            .HasForeignKey(x => x.BuildingMaterialId);
    }
}
