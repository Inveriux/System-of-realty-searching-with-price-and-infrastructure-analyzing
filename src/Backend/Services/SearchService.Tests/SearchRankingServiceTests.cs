using SearchService.DTOs.Search;
using SearchService.Models;
using SearchService.Services.Ranking;
using Xunit;

namespace SearchService.Tests;

public sealed class SearchRankingServiceTests
{
    private readonly SearchRankingService _rankingService = new();
    private readonly SearchProfileProvider _profileProvider = new();

    [Fact]
    public void Rank_ShouldPreferUrbanListingForUrbanProfile()
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

        var urbanWeights = _profileProvider.GetWeights(request);
        var urbanListing = CreateListing("urban", transportScore: 84m, commerceScore: 80m, leisureScore: 72m, stopWalk: 4m, metroWalk: 9m, storeWalk: 5m);
        var suburbanListing = CreateListing("suburban", transportScore: 55m, commerceScore: 62m, leisureScore: 48m, stopWalk: 11m, metroWalk: 20m, storeWalk: 12m);

        var urbanRank = _rankingService.Rank(urbanListing, request, urbanWeights);
        var suburbanRank = _rankingService.Rank(suburbanListing, request, urbanWeights);

        Assert.True(urbanRank.FinalScore > suburbanRank.FinalScore);
        Assert.Contains(urbanRank.Highlights, x => x.Contains("Остановка", StringComparison.Ordinal));
    }

    [Fact]
    public void Rank_ShouldPreferFamilyListingForFamilyProfile()
    {
        var request = new SearchListingsRequestDto
        {
            SearchProfile = "family",
            Sort = "relevance",
            MaxSchoolWalkMinutes = 15m,
            MaxKindergartenWalkMinutes = 15m,
            MaxParkWalkMinutes = 15m
        };

        var familyWeights = _profileProvider.GetWeights(request);
        var familyListing = CreateListing("family", educationScore: 88m, healthcareScore: 76m, leisureScore: 70m, schoolWalk: 6m, kindergartenWalk: 7m, parkWalk: 8m);
        var urbanListing = CreateListing("urban", educationScore: 58m, healthcareScore: 60m, leisureScore: 72m, schoolWalk: 16m, kindergartenWalk: 18m, parkWalk: 20m);

        var familyRank = _rankingService.Rank(familyListing, request, familyWeights);
        var urbanRank = _rankingService.Rank(urbanListing, request, familyWeights);

        Assert.True(familyRank.FinalScore > urbanRank.FinalScore);
    }

    [Fact]
    public void Rank_ShouldRewardCloserPriceToRequestedBandCenter()
    {
        var request = new SearchListingsRequestDto
        {
            PriceMin = 80_000m,
            PriceMax = 100_000m,
            AreaMin = 55m,
            AreaMax = 75m,
            Sort = "relevance"
        };

        var weights = _profileProvider.GetWeights(request);
        var centeredPrice = CreateListing("centered", price: 91_000m, area: 64m);
        var edgePrice = CreateListing("edge", price: 80_500m, area: 64m);

        var centeredRank = _rankingService.Rank(centeredPrice, request, weights);
        var edgeRank = _rankingService.Rank(edgePrice, request, weights);

        Assert.True(centeredRank.Breakdown.PriceFitScore > edgeRank.Breakdown.PriceFitScore);
        Assert.True(centeredRank.FinalScore > edgeRank.FinalScore);
    }

    [Fact]
    public void GetWeights_ShouldPreferExplicitWeightsOverProfilePreset()
    {
        var request = new SearchListingsRequestDto
        {
            SearchProfile = "urban",
            TransportWeight = 0.4m,
            EducationWeight = 1.7m,
            LeisureWeight = 1.6m
        };

        var weights = _profileProvider.GetWeights(request);

        Assert.Equal(0.4m, weights.TransportWeight);
        Assert.Equal(1.7m, weights.EducationWeight);
        Assert.Equal(1.6m, weights.LeisureWeight);
        Assert.Equal(0.90m, weights.PriceWeight);
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
        decimal storeWalk = 7m,
        decimal schoolWalk = 10m,
        decimal kindergartenWalk = 11m,
        decimal parkWalk = 9m)
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
            NearestSchoolWalkMinutes = schoolWalk,
            NearestKindergartenWalkMinutes = kindergartenWalk,
            NearestParkWalkMinutes = parkWalk,
            PublishedDate = DateTimeOffset.UtcNow.AddDays(-2),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
}
