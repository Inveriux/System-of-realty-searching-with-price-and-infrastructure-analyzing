using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingMediaConfiguration : IEntityTypeConfiguration<ListingMedia>
{
    private const string TableName = "listing_media";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<ListingMedia> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ListingId)
            .IsRequired()
            .ValueGeneratedNever();
            
        builder.HasOne(x => x.Listing)
            .WithMany(x => x.Media)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(StringMaxLength);
    }
}
