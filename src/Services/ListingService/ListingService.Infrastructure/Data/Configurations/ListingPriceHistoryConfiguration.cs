using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingPriceHistoryConfiguration : IEntityTypeConfiguration<ListingPriceHistory>
{
    private const string TableName = "listing_price_history";
    private const int StringMaxLength = 100;

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
            .IsRequired();

        builder.Property(x => x.PriceCurrency)
            .HasMaxLength(StringMaxLength)
            .IsRequired();

        builder.Property(x => x.PreviousPrice)
            .IsRequired();
    }
}
