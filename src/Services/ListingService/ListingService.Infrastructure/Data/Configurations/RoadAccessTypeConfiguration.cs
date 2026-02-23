using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Models;

namespace ListingService.Infrastructure.Data.Configurations;

internal class RoadAccessTypeConfiguration : IEntityTypeConfiguration<RoadAccessType>
{
    private const string TableName = "road_access_type";
    private const int StringMaxLength = 100;

    public void Configure(EntityTypeBuilder<RoadAccessType> builder)
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
