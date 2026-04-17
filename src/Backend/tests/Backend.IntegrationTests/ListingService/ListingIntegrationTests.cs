using Backend.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace Backend.IntegrationTests.ListingServiceTests;

[Trait("Category", "Integration")]
public sealed class ListingIntegrationTests : IClassFixture<ListingServiceFactory>
{
    private readonly ListingServiceFactory _factory;

    public ListingIntegrationTests(ListingServiceFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task RootEndpoint_ShouldReturnOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task GetListings_ShouldReturnOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/listings?page=0&pageSize=10");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task CreateListing_WithoutToken_ShouldReturnUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/listings", CreateListingCommandEnvelope(Guid.NewGuid(), "Unauthorized listing"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task CreateListing_WithValidPayload_ShouldCreateListing()
    {
        var agentId = Guid.NewGuid();
        using var client = _factory.CreateAuthenticatedClient(agentId);

        var response = await client.PostAsJsonAsync("/api/listings", CreateListingCommandEnvelope(agentId, "Квартира для smoke"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ResultEnvelope<Guid>>();
        payload!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateListing_WithInvalidPayload_ShouldReturnBadRequest()
    {
        var agentId = Guid.NewGuid();
        using var client = _factory.CreateAuthenticatedClient(agentId);

        var command = CreateListingCommandEnvelope(agentId, string.Empty);
        ((Dictionary<string, object?>)command["listingDto"]!)["priceAmount"] = 0m;

        var response = await client.PostAsJsonAsync("/api/listings", command);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetListingById_AfterCreation_ShouldReturnCreatedListing()
    {
        var agentId = Guid.NewGuid();
        using var client = _factory.CreateAuthenticatedClient(agentId);

        var createResponse = await client.PostAsJsonAsync("/api/listings", CreateListingCommandEnvelope(agentId, "Квартира для чтения"));
        var createPayload = await createResponse.Content.ReadFromJsonAsync<ResultEnvelope<Guid>>();
        var listingId = createPayload!.Data;

        using var anonymousClient = _factory.CreateClient();
        var getResponse = await anonymousClient.GetAsync($"/api/listings/{listingId}");

        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var getPayload = await getResponse.Content.ReadFromJsonAsync<ResultEnvelope<ListingPayload>>();
        getPayload!.Data!.Id.Should().Be(listingId);
        getPayload.Data.Title.Should().Be("Квартира для чтения");
    }

    [Fact]
    public async Task GetListings_WhenPageSizeIsLimited_ShouldReturnPagedSubset()
    {
        var agentId = Guid.NewGuid();
        using var client = _factory.CreateAuthenticatedClient(agentId);

        await client.PostAsJsonAsync("/api/listings", CreateListingCommandEnvelope(agentId, "Квартира A"));
        await client.PostAsJsonAsync("/api/listings", CreateListingCommandEnvelope(agentId, "Квартира B"));

        using var anonymousClient = _factory.CreateClient();
        var response = await anonymousClient.GetAsync("/api/listings?page=0&pageSize=1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ResultEnvelope<PagedListingPayload>>();
        payload!.Data.Should().NotBeNull();
        payload.Data!.PageSize.Should().Be(1);
        payload.Data.Items.Should().HaveCount(1);
    }

    private static Dictionary<string, object?> CreateListingCommandEnvelope(Guid agentId, string title)
        => new()
        {
            ["listingDto"] = new Dictionary<string, object?>
            {
                ["agentId"] = agentId,
                ["addressText"] = "Минск, ул. Примерная, 1",
                ["title"] = title,
                ["description"] = "Тестовое объявление",
                ["priceAmount"] = 150000m,
                ["priceAmountByn"] = 150000m,
                ["areaTotal"] = 63.5m,
                ["buildYear"] = 2018,
                ["publishedDate"] = DateTimeOffset.UtcNow,
                ["addressId"] = Guid.NewGuid(),
                ["buildingType"] = "Apartment Building (Multi-family)",
                ["listingType"] = "Sale",
                ["status"] = "Active",
                ["listingDetailsType"] = "Apartment",
                ["mediaUrls"] = new[] { "https://images.test/listing-1.jpg" },
                ["listingDetails"] = new Dictionary<string, object?>
                {
                    ["rooms"] = 2,
                    ["bedrooms"] = 1,
                    ["floorsTotal"] = 12,
                    ["floor"] = 5,
                    ["areaLiving"] = 34.1m,
                    ["areaKitchen"] = 11.2m,
                    ["hasBalcony"] = true,
                    ["hasElevator"] = true,
                    ["bathroomType"] = "Separate",
                    ["renovationType"] = "Cosmetic",
                    ["buildingMaterial"] = "Brick"
                }
            }
        };

    private sealed class ListingPayload
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    private sealed class PagedListingPayload
    {
        public IReadOnlyList<ListingPayload> Items { get; init; } = [];
        public int PageSize { get; init; }
    }
}
