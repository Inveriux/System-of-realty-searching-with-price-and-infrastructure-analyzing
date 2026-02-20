using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class ListingLandDetailsConfiguration : IEntityTypeConfiguration<LandDetails>
{
    private const string TableName = "land_details";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<LandDetails> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.ListingId);

        builder.Property(x => x.ListingId)
            .IsRequired()
            .ValueGeneratedNever();
        builder.HasOne(x => x.Listing)
            .WithOne()
            .HasForeignKey<LandDetails>(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.LandCategoryId)
            .IsRequired();

        builder.Property(x => x.AllowedUse)
            .HasMaxLength(StringMaxLength);
        builder.Property(x => x.RoadAccessTypeId)
            .IsRequired();

        builder.HasOne(x => x.LandCategory)
            .WithMany()
            .HasForeignKey(x => x.LandCategoryId);

        builder.HasOne(x => x.RoadAccessType)
            .WithMany()
            .HasForeignKey(x => x.RoadAccessTypeId);
    }
}
