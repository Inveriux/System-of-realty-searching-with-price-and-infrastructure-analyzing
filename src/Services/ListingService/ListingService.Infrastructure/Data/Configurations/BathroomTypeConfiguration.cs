using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class BathroomTypeConfiguration : IEntityTypeConfiguration<BathroomType>
{
    private const string TableName = "bathroom_type";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<BathroomType> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(StringMaxLength)
            .IsRequired();
    }
}
