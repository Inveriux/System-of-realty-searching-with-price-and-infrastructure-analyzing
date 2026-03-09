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

        builder.Property(x => x.AllowedUse)
            .HasMaxLength(StringMaxLength);

        builder.HasOne(x => x.LandCategory)
            .WithMany()
            .HasForeignKey(x => x.LandCategoryId);
        builder.Navigation(x => x.LandCategory).AutoInclude();

        builder.HasOne(x => x.RoadAccessType)
            .WithMany()
            .HasForeignKey(x => x.RoadAccessTypeId);
        builder.Navigation(x => x.RoadAccessType).AutoInclude();

        builder.Ignore(x => x.ListingDetailsType);
    }
}
