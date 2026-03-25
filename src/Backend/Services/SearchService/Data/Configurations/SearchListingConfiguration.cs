using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchService.Models;

namespace SearchService.Data.Configurations;

internal sealed class SearchListingConfiguration : IEntityTypeConfiguration<SearchListing>
{
    public void Configure(EntityTypeBuilder<SearchListing> builder)
    {
        builder.ToTable("search_listing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.AddressText).HasMaxLength(500);
        builder.Property(x => x.ListingType).HasMaxLength(100);
        builder.Property(x => x.BuildingType).HasMaxLength(100);
        builder.Property(x => x.ListingDetailsType).HasMaxLength(100);
        builder.Property(x => x.PriceCurrency).HasMaxLength(10);
        builder.Property(x => x.MainMediaUrl).HasMaxLength(1000);
        builder.Property(x => x.Latitude).HasPrecision(10, 6);
        builder.Property(x => x.Longitude).HasPrecision(10, 6);
        builder.Property(x => x.PriceAmount).HasPrecision(18, 2);
        builder.Property(x => x.PriceAmountByn).HasPrecision(18, 2);
        builder.Property(x => x.PriceAmountUsd).HasPrecision(18, 2);
        builder.Property(x => x.AreaTotal).HasPrecision(18, 2);
        builder.Property(x => x.InfrastructureScore).HasPrecision(10, 4);
        builder.Property(x => x.EducationScore).HasPrecision(10, 4);
        builder.Property(x => x.HealthcareScore).HasPrecision(10, 4);
        builder.Property(x => x.TransportScore).HasPrecision(10, 4);
        builder.Property(x => x.CommerceScore).HasPrecision(10, 4);
        builder.Property(x => x.LeisureScore).HasPrecision(10, 4);
        builder.Property(x => x.EducationNearestMeters).HasPrecision(10, 2);
        builder.Property(x => x.HealthcareNearestMeters).HasPrecision(10, 2);
        builder.Property(x => x.TransportNearestMeters).HasPrecision(10, 2);
        builder.Property(x => x.CommerceNearestMeters).HasPrecision(10, 2);
        builder.Property(x => x.LeisureNearestMeters).HasPrecision(10, 2);
        builder.Property(x => x.NearestPharmacyWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestStoreWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestStopWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestParkingWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestSchoolWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestKindergartenWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestClinicDriveMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestHospitalDriveMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestParkWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestFitnessWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestFitnessDriveMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestMuseumDriveMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestTheaterDriveMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestCinemaDriveMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestMetroWalkMinutes).HasPrecision(10, 2);
        builder.Property(x => x.NearestMetroName).HasMaxLength(300);
        builder.Property(x => x.SearchFeaturesJson).HasColumnType("jsonb");

        builder.HasIndex(x => x.ListingId).IsUnique();
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.AgentId);
        builder.HasIndex(x => x.ListingType);
        builder.HasIndex(x => x.BuildingType);
        builder.HasIndex(x => x.PriceAmount);
        builder.HasIndex(x => x.PublishedDate);
        builder.HasIndex(x => x.InfrastructureScore);
        builder.HasIndex(x => x.NearestStoreWalkMinutes);
        builder.HasIndex(x => x.NearestPharmacyWalkMinutes);
        builder.HasIndex(x => x.NearestStopWalkMinutes);
        builder.HasIndex(x => x.NearestParkingWalkMinutes);
        builder.HasIndex(x => x.NearestSchoolWalkMinutes);
        builder.HasIndex(x => x.NearestKindergartenWalkMinutes);
        builder.HasIndex(x => x.NearestClinicDriveMinutes);
        builder.HasIndex(x => x.NearestHospitalDriveMinutes);
        builder.HasIndex(x => x.NearestParkWalkMinutes);
        builder.HasIndex(x => x.NearestFitnessWalkMinutes);
        builder.HasIndex(x => x.NearestMuseumDriveMinutes);
        builder.HasIndex(x => x.NearestTheaterDriveMinutes);
        builder.HasIndex(x => x.NearestCinemaDriveMinutes);
        builder.HasIndex(x => x.NearestMetroWalkMinutes);
        builder.HasIndex(x => x.MetroFoundInPrimaryRadius);
    }
}
