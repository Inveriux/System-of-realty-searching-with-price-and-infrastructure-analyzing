using Microsoft.EntityFrameworkCore;
using PricingAnalyticsService.Models;

namespace PricingAnalyticsService.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<PricingComparableListing> ComparableListings => Set<PricingComparableListing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PricingComparableListing>(builder =>
        {
            builder.ToTable("search_listing");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AddressText).HasMaxLength(500);
            builder.Property(x => x.ListingType).HasMaxLength(100);
            builder.Property(x => x.BuildingType).HasMaxLength(100);
            builder.Property(x => x.ListingDetailsType).HasMaxLength(100);
            builder.Property(x => x.PriceCurrency).HasMaxLength(10);
            builder.Property(x => x.Latitude).HasPrecision(10, 6);
            builder.Property(x => x.Longitude).HasPrecision(10, 6);
            builder.Property(x => x.PriceAmount).HasPrecision(18, 2);
            builder.Property(x => x.PriceAmountByn).HasPrecision(18, 2);
            builder.Property(x => x.AreaTotal).HasPrecision(18, 2);
        });
    }
}
