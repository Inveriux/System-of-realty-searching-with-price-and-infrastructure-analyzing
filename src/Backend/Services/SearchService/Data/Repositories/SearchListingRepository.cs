using BasicBuildingBlocks.Collections;
using Microsoft.EntityFrameworkCore;
using SearchService.Data.Repositories.Abstractions;
using SearchService.DTOs.Search;
using SearchService.Models;

namespace SearchService.Data.Repositories;

public sealed class SearchListingRepository(ApplicationDbContext context) : ISearchListingRepository
{
    public Task<SearchListing?> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken)
        => context.SearchListings.FirstOrDefaultAsync(x => x.ListingId == listingId, cancellationToken);

    public async Task<IReadOnlyCollection<SearchListing>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken)
        => await context.SearchListings
            .Where(x => x.LocationId == locationId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<SearchListing>> SearchAsync(
        SearchListingsRequestDto request,
        CancellationToken cancellationToken)
    {
        var searchQuery = context.SearchListings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var pattern = $"%{request.Query.Trim()}%";
            searchQuery = searchQuery.Where(x =>
                EF.Functions.ILike(x.Title, pattern) ||
                (x.Description != null && EF.Functions.ILike(x.Description, pattern)) ||
                (x.AddressText != null && EF.Functions.ILike(x.AddressText, pattern)));
        }

        if (request.LocationId.HasValue)
            searchQuery = searchQuery.Where(x => x.LocationId == request.LocationId.Value);

        if (!string.IsNullOrWhiteSpace(request.ListingType))
            searchQuery = searchQuery.Where(x => x.ListingType == request.ListingType);

        if (!string.IsNullOrWhiteSpace(request.BuildingType))
            searchQuery = searchQuery.Where(x => x.BuildingType == request.BuildingType);

        if (request.MinPrice.HasValue)
            searchQuery = searchQuery.Where(x => x.PriceAmount >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            searchQuery = searchQuery.Where(x => x.PriceAmount <= request.MaxPrice.Value);

        if (request.MinArea.HasValue)
            searchQuery = searchQuery.Where(x => x.AreaTotal >= request.MinArea.Value);

        if (request.MaxArea.HasValue)
            searchQuery = searchQuery.Where(x => x.AreaTotal <= request.MaxArea.Value);

        if (request.MinInfrastructureScore.HasValue)
            searchQuery = searchQuery.Where(x => x.InfrastructureScore >= request.MinInfrastructureScore.Value);

        if (request.MinEducationScore.HasValue)
            searchQuery = searchQuery.Where(x => x.EducationScore >= request.MinEducationScore.Value);

        if (request.MinHealthcareScore.HasValue)
            searchQuery = searchQuery.Where(x => x.HealthcareScore >= request.MinHealthcareScore.Value);

        if (request.MinTransportScore.HasValue)
            searchQuery = searchQuery.Where(x => x.TransportScore >= request.MinTransportScore.Value);

        if (request.MinCommerceScore.HasValue)
            searchQuery = searchQuery.Where(x => x.CommerceScore >= request.MinCommerceScore.Value);

        if (request.MinLeisureScore.HasValue)
            searchQuery = searchQuery.Where(x => x.LeisureScore >= request.MinLeisureScore.Value);

        if (request.MinEducationCount.HasValue)
            searchQuery = searchQuery.Where(x => (x.EducationCount ?? 0) >= request.MinEducationCount.Value);

        if (request.MinHealthcareCount.HasValue)
            searchQuery = searchQuery.Where(x => (x.HealthcareCount ?? 0) >= request.MinHealthcareCount.Value);

        if (request.MinTransportCount.HasValue)
            searchQuery = searchQuery.Where(x => (x.TransportCount ?? 0) >= request.MinTransportCount.Value);

        if (request.MinCommerceCount.HasValue)
            searchQuery = searchQuery.Where(x => (x.CommerceCount ?? 0) >= request.MinCommerceCount.Value);

        if (request.MinLeisureCount.HasValue)
            searchQuery = searchQuery.Where(x => (x.LeisureCount ?? 0) >= request.MinLeisureCount.Value);

        if (request.MaxEducationNearestMeters.HasValue)
            searchQuery = searchQuery.Where(x => x.EducationNearestMeters != null && x.EducationNearestMeters <= request.MaxEducationNearestMeters.Value);

        if (request.MaxHealthcareNearestMeters.HasValue)
            searchQuery = searchQuery.Where(x => x.HealthcareNearestMeters != null && x.HealthcareNearestMeters <= request.MaxHealthcareNearestMeters.Value);

        if (request.MaxTransportNearestMeters.HasValue)
            searchQuery = searchQuery.Where(x => x.TransportNearestMeters != null && x.TransportNearestMeters <= request.MaxTransportNearestMeters.Value);

        if (request.MaxCommerceNearestMeters.HasValue)
            searchQuery = searchQuery.Where(x => x.CommerceNearestMeters != null && x.CommerceNearestMeters <= request.MaxCommerceNearestMeters.Value);

        if (request.MaxLeisureNearestMeters.HasValue)
            searchQuery = searchQuery.Where(x => x.LeisureNearestMeters != null && x.LeisureNearestMeters <= request.MaxLeisureNearestMeters.Value);

        if (request.MaxStoreWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestStoreWalkMinutes != null && x.NearestStoreWalkMinutes <= request.MaxStoreWalkMinutes.Value);

        if (request.MaxPharmacyWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestPharmacyWalkMinutes != null && x.NearestPharmacyWalkMinutes <= request.MaxPharmacyWalkMinutes.Value);

        if (request.MaxStopWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestStopWalkMinutes != null && x.NearestStopWalkMinutes <= request.MaxStopWalkMinutes.Value);

        if (request.MaxParkingWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestParkingWalkMinutes != null && x.NearestParkingWalkMinutes <= request.MaxParkingWalkMinutes.Value);

        if (request.MaxSchoolWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestSchoolWalkMinutes != null && x.NearestSchoolWalkMinutes <= request.MaxSchoolWalkMinutes.Value);

        if (request.MaxKindergartenWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestKindergartenWalkMinutes != null && x.NearestKindergartenWalkMinutes <= request.MaxKindergartenWalkMinutes.Value);

        if (request.MaxClinicDriveMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestClinicDriveMinutes != null && x.NearestClinicDriveMinutes <= request.MaxClinicDriveMinutes.Value);

        if (request.MaxHospitalDriveMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestHospitalDriveMinutes != null && x.NearestHospitalDriveMinutes <= request.MaxHospitalDriveMinutes.Value);

        if (request.MaxParkWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestParkWalkMinutes != null && x.NearestParkWalkMinutes <= request.MaxParkWalkMinutes.Value);

        if (request.MaxFitnessWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestFitnessWalkMinutes != null && x.NearestFitnessWalkMinutes <= request.MaxFitnessWalkMinutes.Value);

        if (request.MaxMuseumDriveMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestMuseumDriveMinutes != null && x.NearestMuseumDriveMinutes <= request.MaxMuseumDriveMinutes.Value);

        if (request.MaxTheaterDriveMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestTheaterDriveMinutes != null && x.NearestTheaterDriveMinutes <= request.MaxTheaterDriveMinutes.Value);

        if (request.MaxCinemaDriveMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestCinemaDriveMinutes != null && x.NearestCinemaDriveMinutes <= request.MaxCinemaDriveMinutes.Value);

        if (request.MaxMetroWalkMinutes.HasValue)
            searchQuery = searchQuery.Where(x => x.NearestMetroWalkMinutes != null && x.NearestMetroWalkMinutes <= request.MaxMetroWalkMinutes.Value);

        if (request.RequireMetroInPrimaryRadius.HasValue)
            searchQuery = searchQuery.Where(x => x.MetroFoundInPrimaryRadius == request.RequireMetroInPrimaryRadius.Value);

        if (HasBounds(request))
        {
            searchQuery = searchQuery.Where(x =>
                x.Latitude != null &&
                x.Longitude != null &&
                x.Latitude >= request.BoundsMinLatitude!.Value &&
                x.Latitude <= request.BoundsMaxLatitude!.Value &&
                x.Longitude >= request.BoundsMinLongitude!.Value &&
                x.Longitude <= request.BoundsMaxLongitude!.Value);
        }

        if (request.CenterLatitude.HasValue && request.CenterLongitude.HasValue && request.RadiusMeters.HasValue)
        {
            var radiusBounds = BuildRadiusBounds(
                request.CenterLatitude.Value,
                request.CenterLongitude.Value,
                request.RadiusMeters.Value);

            searchQuery = searchQuery.Where(x =>
                x.Latitude != null &&
                x.Longitude != null &&
                x.Latitude >= radiusBounds.MinLatitude &&
                x.Latitude <= radiusBounds.MaxLatitude &&
                x.Longitude >= radiusBounds.MinLongitude &&
                x.Longitude <= radiusBounds.MaxLongitude);
        }

        var totalCount = await searchQuery.LongCountAsync(cancellationToken);
        var sortedQuery = ApplySorting(searchQuery, request.SortBy);
        var items = await sortedQuery
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SearchListing>(items, request.Page, request.PageSize, totalCount);
    }

    public async Task UpsertAsync(SearchListing listing, CancellationToken cancellationToken)
    {
        var existing = await context.SearchListings.FirstOrDefaultAsync(x => x.ListingId == listing.ListingId, cancellationToken);
        if (existing is null)
        {
            await context.SearchListings.AddAsync(listing, cancellationToken);
            return;
        }

        existing.Title = listing.Title;
        existing.AgentId = listing.AgentId;
        existing.Description = listing.Description;
        existing.AddressText = listing.AddressText;
        existing.LocationId = listing.LocationId;
        existing.Latitude = listing.Latitude;
        existing.Longitude = listing.Longitude;
        existing.ListingType = listing.ListingType;
        existing.BuildingType = listing.BuildingType;
        existing.ListingDetailsType = listing.ListingDetailsType;
        existing.PriceAmount = listing.PriceAmount;
        existing.PriceAmountByn = listing.PriceAmountByn;
        existing.PriceAmountUsd = listing.PriceAmountUsd;
        existing.PriceCurrency = listing.PriceCurrency;
        existing.AreaTotal = listing.AreaTotal;
        existing.MainMediaUrl = listing.MainMediaUrl;
        existing.PublishedDate = listing.PublishedDate;
        existing.InfrastructureScore = listing.InfrastructureScore;
        existing.EducationScore = listing.EducationScore;
        existing.HealthcareScore = listing.HealthcareScore;
        existing.TransportScore = listing.TransportScore;
        existing.CommerceScore = listing.CommerceScore;
        existing.LeisureScore = listing.LeisureScore;
        existing.EducationCount = listing.EducationCount;
        existing.HealthcareCount = listing.HealthcareCount;
        existing.TransportCount = listing.TransportCount;
        existing.CommerceCount = listing.CommerceCount;
        existing.LeisureCount = listing.LeisureCount;
        existing.EducationNearestMeters = listing.EducationNearestMeters;
        existing.HealthcareNearestMeters = listing.HealthcareNearestMeters;
        existing.TransportNearestMeters = listing.TransportNearestMeters;
        existing.CommerceNearestMeters = listing.CommerceNearestMeters;
        existing.LeisureNearestMeters = listing.LeisureNearestMeters;
        existing.PharmacyCountPrimary = listing.PharmacyCountPrimary;
        existing.StoreCountPrimary = listing.StoreCountPrimary;
        existing.StopsCountPrimary = listing.StopsCountPrimary;
        existing.ParkingCountPrimary = listing.ParkingCountPrimary;
        existing.SchoolCountPrimary = listing.SchoolCountPrimary;
        existing.KindergartenCountPrimary = listing.KindergartenCountPrimary;
        existing.ClinicCountPrimary = listing.ClinicCountPrimary;
        existing.HospitalCountPrimary = listing.HospitalCountPrimary;
        existing.NearestPharmacyWalkMinutes = listing.NearestPharmacyWalkMinutes;
        existing.NearestStoreWalkMinutes = listing.NearestStoreWalkMinutes;
        existing.NearestStopWalkMinutes = listing.NearestStopWalkMinutes;
        existing.NearestParkingWalkMinutes = listing.NearestParkingWalkMinutes;
        existing.NearestSchoolWalkMinutes = listing.NearestSchoolWalkMinutes;
        existing.NearestKindergartenWalkMinutes = listing.NearestKindergartenWalkMinutes;
        existing.NearestClinicDriveMinutes = listing.NearestClinicDriveMinutes;
        existing.NearestHospitalDriveMinutes = listing.NearestHospitalDriveMinutes;
        existing.ParksCountPrimary = listing.ParksCountPrimary;
        existing.NearestParkWalkMinutes = listing.NearestParkWalkMinutes;
        existing.FitnessCountPrimary = listing.FitnessCountPrimary;
        existing.NearestFitnessWalkMinutes = listing.NearestFitnessWalkMinutes;
        existing.NearestFitnessDriveMinutes = listing.NearestFitnessDriveMinutes;
        existing.MuseumsCountPrimary = listing.MuseumsCountPrimary;
        existing.NearestMuseumDriveMinutes = listing.NearestMuseumDriveMinutes;
        existing.TheatersCountPrimary = listing.TheatersCountPrimary;
        existing.NearestTheaterDriveMinutes = listing.NearestTheaterDriveMinutes;
        existing.CinemasCountPrimary = listing.CinemasCountPrimary;
        existing.NearestCinemaDriveMinutes = listing.NearestCinemaDriveMinutes;
        existing.NearestMetroWalkMinutes = listing.NearestMetroWalkMinutes;
        existing.NearestMetroName = listing.NearestMetroName;
        existing.MetroFoundInPrimaryRadius = listing.MetroFoundInPrimaryRadius;
        existing.SearchFeaturesJson = listing.SearchFeaturesJson;
        existing.UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Remove(SearchListing listing)
        => context.SearchListings.Remove(listing);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);

    private static bool HasBounds(SearchListingsRequestDto request)
        => request.BoundsMinLatitude.HasValue &&
           request.BoundsMinLongitude.HasValue &&
           request.BoundsMaxLatitude.HasValue &&
           request.BoundsMaxLongitude.HasValue;

    private static IQueryable<SearchListing> ApplySorting(IQueryable<SearchListing> query, string? sortBy)
        => (sortBy ?? "published_desc").Trim().ToLowerInvariant() switch
        {
            "price_asc" => query
                .OrderBy(x => x.PriceAmount == null)
                .ThenBy(x => x.PriceAmount)
                .ThenByDescending(x => x.PublishedDate),
            "price_desc" => query
                .OrderByDescending(x => x.PriceAmount)
                .ThenByDescending(x => x.PublishedDate),
            "infrastructure_desc" => query
                .OrderByDescending(x => x.InfrastructureScore ?? 0m)
                .ThenByDescending(x => x.PublishedDate),
            "published_desc" => query
                .OrderByDescending(x => x.PublishedDate)
                .ThenByDescending(x => x.InfrastructureScore ?? 0m),
            _ => query
                .OrderByDescending(x => x.PublishedDate)
                .ThenByDescending(x => x.InfrastructureScore ?? 0m)
        };

    private static RadiusBounds BuildRadiusBounds(decimal centerLatitude, decimal centerLongitude, decimal radiusMeters)
    {
        const decimal metersPerLatitudeDegree = 111_320m;
        var latitudeDelta = radiusMeters / metersPerLatitudeDegree;
        var latitudeRadians = (double)centerLatitude * Math.PI / 180d;
        var longitudeMetersPerDegree = Math.Max(1m, metersPerLatitudeDegree * (decimal)Math.Cos(latitudeRadians));
        var longitudeDelta = radiusMeters / longitudeMetersPerDegree;

        return new RadiusBounds(
            centerLatitude - latitudeDelta,
            centerLongitude - longitudeDelta,
            centerLatitude + latitudeDelta,
            centerLongitude + longitudeDelta);
    }

    private sealed record RadiusBounds(
        decimal MinLatitude,
        decimal MinLongitude,
        decimal MaxLatitude,
        decimal MaxLongitude);
}
