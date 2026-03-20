using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    private const string TableName = "listing";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.AgentId)
            .IsRequired();

        builder.Property(x => x.BuildingTypeId)
            .IsRequired();

        builder.Property(x => x.ListingTypeId)
            .IsRequired();

        builder.Property(x => x.AddressId)
            .IsRequired();

        builder.Property(x => x.AddressText)
            .HasMaxLength(StringMaxLength)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasPrecision(10, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(10, 6);

        builder.Property(x => x.Title)
            .HasMaxLength(StringMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(StringMaxLength)
            .IsRequired();
            
        builder.Property(x => x.PriceCurrency)
            .HasMaxLength(StringMaxLength);

        builder.Property(x => x.PricePeriod)
            .HasMaxLength(StringMaxLength);

        builder.Property(x => x.AreaTotal)
            .IsRequired();
        builder.Ignore(x => x.Status);

        builder.HasIndex(x => x.LocationId);

        builder.HasOne(x => x.BuildingType)
            .WithMany()
            .HasForeignKey(x => x.BuildingTypeId);

        builder.HasOne(x => x.ListingType)
            .WithMany()
            .HasForeignKey(x => x.ListingTypeId);

        builder.HasMany(x => x.Media)
            .WithOne(x => x.Listing)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PriceHistory)
            .WithOne(x => x.Listing)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.ListingDetails);

    }
}
