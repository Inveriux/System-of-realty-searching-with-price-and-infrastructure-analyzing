using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingPriceHistoryConfiguration : IEntityTypeConfiguration<ListingPriceHistory>
{
    private const string TableName = "listing_price_history";
    private const int StringMaxLength = 5;

    public void Configure(EntityTypeBuilder<ListingPriceHistory> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ListingId)
            .IsRequired();
        builder.HasOne(x => x.Listing)
            .WithMany(x => x.PriceHistory)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade); //TODO Is need cascade delete?

        builder.Property(x => x.PriceAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PriceAmountByn)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PriceAmountUsd)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PriceCurrency)
            .HasMaxLength(StringMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.ChangedAt);
        builder.HasIndex(x => x.ListingId);
    }
}
