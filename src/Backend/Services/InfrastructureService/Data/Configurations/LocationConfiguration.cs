using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureService.Data.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("location");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AddressText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.NormalizedAddressText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasPrecision(10, 6)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .HasPrecision(10, 6)
            .IsRequired();

        builder.Property(x => x.Country).HasMaxLength(200);
        builder.Property(x => x.Region).HasMaxLength(200);
        builder.Property(x => x.City).HasMaxLength(200);
        builder.Property(x => x.Street).HasMaxLength(200);
        builder.Property(x => x.HouseNumber).HasMaxLength(50);

        builder.HasIndex(x => x.NormalizedAddressText).IsUnique();
        builder.HasIndex(x => new { x.Latitude, x.Longitude });
    }
}
