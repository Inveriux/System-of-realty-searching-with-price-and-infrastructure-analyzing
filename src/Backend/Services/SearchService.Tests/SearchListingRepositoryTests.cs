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
        decimal cinemaDrive)
        => new()
        {
            Id = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            Title = title,
            AreaTotal = 50m,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            PublishedDate = DateTimeOffset.UtcNow,
            NearestParkWalkMinutes = parkWalk,
            NearestFitnessWalkMinutes = fitnessWalk,
            NearestMuseumDriveMinutes = museumDrive,
            NearestTheaterDriveMinutes = theaterDrive,
            NearestCinemaDriveMinutes = cinemaDrive
        };
}
