using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchService.Models;

namespace SearchService.Data.Configurations;

internal sealed class SearchListingConfiguration : IEntityTypeConfiguration<SearchListing>
{
    public void Configure(EntityTypeBuilder<SearchListing> builder)
    {
        builder.ToTable("search_listing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.AddressText).HasMaxLength(500);
        builder.Property(x => x.ListingType).HasMaxLength(100);
        builder.Property(x => x.BuildingType).HasMaxLength(100);
        builder.Property(x => x.PriceCurrency).HasMaxLength(10);
        builder.Property(x => x.MainMediaUrl).HasMaxLength(1000);
        builder.Property(x => x.Latitude).HasPrecision(10, 6);
        builder.Property(x => x.Longitude).HasPrecision(10, 6);
        builder.Property(x => x.PriceAmount).HasPrecision(18, 2);
        builder.Property(x => x.AreaTotal).HasPrecision(18, 2);
        builder.Property(x => x.InfrastructureScore).HasPrecision(10, 4);
        builder.Property(x => x.SearchFeaturesJson).HasColumnType("jsonb");

        builder.HasIndex(x => x.ListingId).IsUnique();
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.ListingType);
    }
}
