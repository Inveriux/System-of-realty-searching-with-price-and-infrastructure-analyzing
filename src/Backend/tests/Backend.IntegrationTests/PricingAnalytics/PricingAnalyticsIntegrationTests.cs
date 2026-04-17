using Backend.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace Backend.IntegrationTests.PricingAnalyticsTests;

[Trait("Category", "Integration")]
public sealed class PricingAnalyticsIntegrationTests : IClassFixture<PricingAnalyticsServiceFactory>
{
    private readonly PricingAnalyticsServiceFactory _factory;

    public PricingAnalyticsIntegrationTests(PricingAnalyticsServiceFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Estimate_ShouldReturnAnalyticsResponse()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/pricing-analytics/estimate", new
        {
            listingType = "sale",
            buildingType = "Apartment Building (Multi-family)",
            listingDetailsType = "Apartment",
            priceAmountByn = 192000m,
            areaTotal = 60m,
            cityName = "Минск"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ResultEnvelope<EstimatePayload>>();
        payload!.Data!.CityAveragePricePerSqm.Should().Be(3200m);
        payload.Data.HasEnoughData.Should().BeTrue();
        payload.Data.Message.Should().Be("Цена близка к рынку.");
    }

    private sealed class EstimatePayload
    {
        public decimal? CityAveragePricePerSqm { get; init; }
        public bool HasEnoughData { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
