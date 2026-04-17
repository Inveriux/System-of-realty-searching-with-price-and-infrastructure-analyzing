using FluentAssertions;
using ListingService.Application.Contracts.Repositories;
using ListingService.Application.UseCases.Listings;
using ListingService.Domain.Models;
using Moq;
using Xunit;

namespace Backend.UnitTests.Listings;

[Trait("Category", "Unit")]
public sealed class GetListingPriceHistoryHandlerTests
{
    [Fact]
    public async Task Handle_WhenHistoryExists_ShouldCalculateTimelineAndPercentChange()
    {
        var listingId = Guid.NewGuid();
        var listing = Listing.Create();
        listing.Id = listingId;
        listing.AreaTotal = 50m;
        listing.PriceAmountByn = 120_000m;

        IReadOnlyCollection<ListingPriceHistory> history =
        [
            new ListingPriceHistory
            {
                Id = Guid.NewGuid(),
                ChangedAt = new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero),
                PriceAmountByn = 100_000m
            },
            new ListingPriceHistory
            {
                Id = Guid.NewGuid(),
                ChangedAt = new DateTimeOffset(2025, 3, 10, 0, 0, 0, TimeSpan.Zero),
                PriceAmountByn = 120_000m
            }
        ];

        var listingRepository = new Mock<IListingRepository>();
        listingRepository.Setup(x => x.GetDetailedByIdAsync(listingId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);
        listingRepository.Setup(x => x.GetPriceHistoryAsync(listingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Listing).Returns(listingRepository.Object);

        var handler = new GetListingPriceHistoryHandler(repositoryManager.Object);

        var result = await handler.Handle(new GetListingPriceHistoryQuery(listingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PercentChangeFromFirst.Should().Be(20m);
        result.Data.History.Should().HaveCount(2);
        result.Data.History.Last().PricePerSqmByn.Should().Be(2400m);
    }
}
