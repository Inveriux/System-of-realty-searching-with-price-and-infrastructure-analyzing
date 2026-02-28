using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingHouseDetailsConfiguration : IEntityTypeConfiguration<HouseDetails>
{
    private const string TableName = "house_details";

    public void Configure(EntityTypeBuilder<HouseDetails> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.ListingId);

        builder.Property(x => x.ListingId)
            .IsRequired()
            .ValueGeneratedNever();
            
        builder.HasOne(x => x.Listing)
            .WithOne()
            .HasForeignKey<HouseDetails>(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.WallMaterialId)
            .IsRequired();

        builder.Property(x => x.RoofMaterialId)
            .IsRequired();

        builder.Property(x => x.HeatingTypeId)
            .IsRequired();

        builder.Property(x => x.WaterSupplyTypeId)
            .IsRequired();

        builder.Property(x => x.SewerTypeId)
            .IsRequired();

        builder.Property(x => x.GasTypeId)
            .IsRequired();

        builder.HasOne(x => x.WallMaterial)
            .WithMany()
            .HasForeignKey(x => x.WallMaterialId);
        builder.Navigation(x => x.WallMaterial).AutoInclude();

        builder.HasOne(x => x.RoofMaterial)
            .WithMany()
            .HasForeignKey(x => x.RoofMaterialId);
        builder.Navigation(x => x.RoofMaterial).AutoInclude();

        builder.HasOne(x => x.HeatingType)
            .WithMany()
            .HasForeignKey(x => x.HeatingTypeId);
        builder.Navigation(x => x.HeatingType).AutoInclude();

        builder.HasOne(x => x.WaterSupplyType)
            .WithMany()
            .HasForeignKey(x => x.WaterSupplyTypeId);
        builder.Navigation(x => x.WaterSupplyType).AutoInclude();

        builder.HasOne(x => x.SewerType)
            .WithMany()
            .HasForeignKey(x => x.SewerTypeId);
        builder.Navigation(x => x.SewerType).AutoInclude();

        builder.HasOne(x => x.GasType)
            .WithMany()
            .HasForeignKey(x => x.GasTypeId);
        builder.Navigation(x => x.GasType).AutoInclude();

    }
}
