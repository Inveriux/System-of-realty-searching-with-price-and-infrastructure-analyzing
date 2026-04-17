using FluentAssertions;
using SearchService.DTOs.Search;
using SearchService.Models;
using SearchService.Services.Ranking;
using Xunit;

namespace Backend.UnitTests.Search;

[Trait("Category", "Unit")]
public sealed class SearchRankingServiceTests
{
    private readonly SearchRankingService _rankingService = new();
    private readonly SearchProfileProvider _profileProvider = new();

    [Fact]
    public void Rank_WhenUrbanProfileIsSelected_ShouldPreferListingWithBetterUrbanSignals()
    {
        var request = new SearchListingsRequestDto
        {
            SearchProfile = "urban",
            Sort = "relevance",
            MaxStopWalkMinutes = 7m,
            MaxMetroWalkMinutes = 15m,
            MaxStoreWalkMinutes = 10m,
            IncludeHighlights = true
        };

        var weights = _profileProvider.GetWeights(request);
        var strongUrban = CreateListing("strong-urban", transportScore: 84m, commerceScore: 80m, leisureScore: 72m, stopWalk: 4m, metroWalk: 9m, storeWalk: 5m);
        var weakUrban = CreateListing("weak-urban", transportScore: 55m, commerceScore: 62m, leisureScore: 48m, stopWalk: 11m, metroWalk: 20m, storeWalk: 12m);

        var strongRank = _rankingService.Rank(strongUrban, request, weights);
        var weakRank = _rankingService.Rank(weakUrban, request, weights);

        strongRank.FinalScore.Should().BeGreaterThan(weakRank.FinalScore);
        strongRank.Highlights.Should().Contain(highlight => highlight.Contains("Остановка", StringComparison.Ordinal));
    }

    [Fact]
    public void GetWeights_WhenExplicitWeightsArePassed_ShouldOverrideProfilePreset()
    {
        var request = new SearchListingsRequestDto
        {
            SearchProfile = "urban",
            TransportWeight = 0.4m,
            EducationWeight = 1.7m,
            LeisureWeight = 1.6m
        };

        var weights = _profileProvider.GetWeights(request);

        weights.TransportWeight.Should().Be(0.4m);
        weights.EducationWeight.Should().Be(1.7m);
        weights.LeisureWeight.Should().Be(1.6m);
        weights.PriceWeight.Should().Be(0.90m);
    }

    private static SearchListing CreateListing(
        string title,
        decimal price = 90_000m,
        decimal area = 65m,
        decimal transportScore = 70m,
        decimal educationScore = 70m,
        decimal healthcareScore = 70m,
        decimal commerceScore = 70m,
        decimal leisureScore = 70m,
        decimal stopWalk = 6m,
        decimal metroWalk = 12m,
        decimal storeWalk = 7m)
        => new()
        {
            Id = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            Title = title,
            ListingType = "Sale",
            BuildingType = "Apartment",
            PriceAmount = price,
            PriceAmountByn = price,
            AreaTotal = area,
            TransportScore = transportScore,
            EducationScore = educationScore,
            HealthcareScore = healthcareScore,
            CommerceScore = commerceScore,
            LeisureScore = leisureScore,
            NearestStopWalkMinutes = stopWalk,
            NearestMetroWalkMinutes = metroWalk,
            NearestStoreWalkMinutes = storeWalk,
            PublishedDate = DateTimeOffset.UtcNow.AddDays(-2),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
}
