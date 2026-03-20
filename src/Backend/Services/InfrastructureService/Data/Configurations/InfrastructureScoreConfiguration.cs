using InfrastructureService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureService.Data.Configurations;

internal sealed class InfrastructureScoreConfiguration : IEntityTypeConfiguration<InfrastructureScore>
{
    public void Configure(EntityTypeBuilder<InfrastructureScore> builder)
    {
        builder.ToTable("infrastructure_score");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.EducationScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.HealthcareScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.TransportScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.CommerceScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.LeisureScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.SearchFeaturesJson).HasColumnType("jsonb");

        builder.HasIndex(x => x.LocationId).IsUnique();

        builder.HasOne(x => x.Location)
            .WithOne()
            .HasForeignKey<InfrastructureScore>(x => x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Snapshot)
            .WithOne(x => x.InfrastructureScore)
            .HasForeignKey<InfrastructureScore>(x => x.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
