using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Data.Repositories;
using SearchService.DTOs.Search;
using SearchService.Models;
using Xunit;

namespace SearchService.Tests;

public sealed class SearchListingRepositoryTests
{
    [Fact]
    public async Task SearchAsync_ShouldFilterByNewLeisureMetrics()
    {
        await using var context = CreateContext();
        context.SearchListings.AddRange(
            CreateListing("near-park", parkWalk: 7m, fitnessWalk: 9m, museumDrive: 12m, theaterDrive: 14m, cinemaDrive: 16m),
            CreateListing("far-park", parkWalk: 18m, fitnessWalk: 19m, museumDrive: 28m, theaterDrive: 30m, cinemaDrive: 32m));
        await context.SaveChangesAsync();

        var repository = new SearchListingRepository(context);
        var result = await repository.SearchAsync(new SearchListingsRequestDto
        {
            MaxParkWalkMinutes = 10m,
            MaxFitnessWalkMinutes = 10m,
            MaxMuseumDriveMinutes = 15m,
            MaxTheaterDriveMinutes = 15m,
            MaxCinemaDriveMinutes = 20m,
            Page = 0,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("near-park", result.Items.Single().Title);
    }

    [Fact]
    public async Task SearchAsync_ShouldSupportOperationAndPropertyAliases()
    {
        await using var context = CreateContext();
        context.SearchListings.AddRange(
            CreateListing("matching", parkWalk: 7m, fitnessWalk: 9m, museumDrive: 12m, theaterDrive: 14m, cinemaDrive: 16m, listingType: "Sale", buildingType: "Apartment"),
            CreateListing("other", parkWalk: 7m, fitnessWalk: 9m, museumDrive: 12m, theaterDrive: 14m, cinemaDrive: 16m, listingType: "Rent", buildingType: "House"));
        await context.SaveChangesAsync();

        var repository = new SearchListingRepository(context);
        var result = await repository.SearchAsync(new SearchListingsRequestDto
        {
            OperationType = "Sale",
            PropertyType = "Apartment",
            Page = 0,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("matching", result.Items.Single().Title);
    }

    [Fact]
    public async Task GetRelevanceCandidatesAsync_ShouldLimitAndFilterCandidatePool()
    {
        await using var context = CreateContext();
        foreach (var index in Enumerable.Range(0, 520))
        {
            context.SearchListings.Add(CreateListing(
                $"listing-{index}",
                parkWalk: 8m,
                fitnessWalk: 9m,
                museumDrive: 12m,
                theaterDrive: 14m,
                cinemaDrive: 16m,
                storeWalk: index % 2 == 0 ? 6m : 18m,
                listingType: "Sale",
                buildingType: "Apartment"));
        }

        await context.SaveChangesAsync();

        var repository = new SearchListingRepository(context);
        var candidates = await repository.GetRelevanceCandidatesAsync(new SearchListingsRequestDto
        {
            OperationType = "Sale",
            PropertyType = "Apartment",
            MaxStoreWalkMinutes = 10m,
            Sort = "relevance",
            Page = 0,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(candidates.TotalCount >= 250);
        Assert.Equal(250, candidates.Items.Count);
        Assert.All(candidates.Items, item => Assert.True(item.NearestStoreWalkMinutes <= 10m));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static SearchListing CreateListing(
        string title,
        decimal parkWalk,
        decimal fitnessWalk,
        decimal museumDrive,
        decimal theaterDrive,
        decimal cinemaDrive,
        decimal storeWalk = 5m,
        string? listingType = null,
        string? buildingType = null)
        => new()
        {
            Id = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            Title = title,
            ListingType = listingType,
            BuildingType = buildingType,
            AreaTotal = 50m,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            PublishedDate = DateTimeOffset.UtcNow,
            NearestStoreWalkMinutes = storeWalk,
            NearestParkWalkMinutes = parkWalk,
            NearestFitnessWalkMinutes = fitnessWalk,
            NearestMuseumDriveMinutes = museumDrive,
            NearestTheaterDriveMinutes = theaterDrive,
            NearestCinemaDriveMinutes = cinemaDrive
        };
}
