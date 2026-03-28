using System.Net;
using System.Text;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Places.Overpass;
using InfrastructureService.Services.Places.Overpass.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace InfrastructureService.Tests;

public sealed class OverpassIntegrationTests
{
    [Fact]
    public void OverpassPoiMapper_ShouldMapImportantOsmTagsToExistingScoringSubcategories()
    {
        var mapper = new OverpassPoiMapper();

        var school = mapper.Map(
            new OverpassElement
            {
                Id = 1,
                Type = "node",
                Latitude = 53.9m,
                Longitude = 27.56m,
                Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["amenity"] = "school",
                    ["name"] = "School 1"
                }
            },
            53.9m,
            27.56m);

        var chemist = mapper.Map(
            new OverpassElement
            {
                Id = 2,
                Type = "node",
                Latitude = 53.9005m,
                Longitude = 27.5605m,
                Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["shop"] = "chemist",
                    ["name"] = "Drogerie"
                }
            },
            53.9m,
            27.56m);

        var metro = mapper.Map(
            new OverpassElement
            {
                Id = 3,
                Type = "node",
                Latitude = 53.901m,
                Longitude = 27.561m,
                Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["railway"] = "subway"
                }
            },
            53.9m,
            27.56m);

        var beauty = mapper.Map(
            new OverpassElement
            {
                Id = 4,
                Type = "node",
                Latitude = 53.9007m,
                Longitude = 27.5607m,
                Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["shop"] = "hairdresser",
                    ["name"] = "Beauty Point"
                }
            },
            53.9m,
            27.56m);

        Assert.NotNull(school);
        Assert.Equal("education", school!.CategoryCode);
        Assert.Equal("school", school.SubcategoryCode);

        Assert.NotNull(chemist);
        Assert.Equal("commerce", chemist!.CategoryCode);
        Assert.Equal("drogerie", chemist.SubcategoryCode);

        Assert.NotNull(metro);
        Assert.Equal("transport", metro!.CategoryCode);
        Assert.Equal("metro_station", metro.SubcategoryCode);
        Assert.Equal("osm-overpass", metro.Poi.Source);

        Assert.NotNull(beauty);
        Assert.Equal("commerce", beauty!.CategoryCode);
        Assert.Equal("beauty_service", beauty.SubcategoryCode);
    }

    [Fact]
    public async Task OverpassApiClient_ShouldBuildAroundQueryAndReadCenters()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            requestBody = await request.Content!.ReadAsStringAsync();
            var responseJson = """
            {
              "version": 0.6,
              "generator": "test",
              "elements": [
                {
                  "type": "node",
                  "id": 10,
                  "lat": 53.9,
                  "lon": 27.56,
                  "tags": {
                    "amenity": "school",
                    "name": "Node school"
                  }
                },
                {
                  "type": "way",
                  "id": 11,
                  "center": {
                    "lat": 53.901,
                    "lon": 27.561
                  },
                  "tags": {
                    "shop": "supermarket",
                    "name": "Way store"
                  }
                }
              ]
            }
            """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://overpass-api.de/api/interpreter"),
            Timeout = TimeSpan.FromSeconds(25)
        };

        var client = new OverpassApiClient(
            httpClient,
            Options.Create(new OverpassOptions
            {
                BaseUrl = "https://overpass-api.de/api/interpreter",
                TimeoutSeconds = 25,
                RetryCount = 0,
                RetryDelayMs = 1,
                UserAgent = "TestAgent/1.0"
            }),
            NullLogger<OverpassApiClient>.Instance);

        var result = await client.QueryNearbyAsync(
            53.9m,
            27.56m,
            1500,
            [
                new OverpassTagFilter { Key = "amenity", Values = ["school", "kindergarten"] },
                new OverpassTagFilter { Key = "shop", Values = ["supermarket"] }
            ],
            CancellationToken.None);

        Assert.NotNull(requestBody);
        var decodedRequestBody = Uri.UnescapeDataString(requestBody!["data=".Length..]);
        Assert.Contains("[out:json]", decodedRequestBody, StringComparison.Ordinal);
        Assert.Contains("node(around:1500,53.9,27.56)[\"amenity\"~\"^(school|kindergarten)$\"]", decodedRequestBody, StringComparison.Ordinal);
        Assert.Contains("way(around:1500,53.9,27.56)[\"shop\"~\"^(supermarket)$\"]", decodedRequestBody, StringComparison.Ordinal);
        Assert.Contains("out center tags;", decodedRequestBody, StringComparison.Ordinal);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Type == "node" && x.Latitude == 53.9m && x.Longitude == 27.56m);
        Assert.Contains(result, x => x.Type == "way" && x.Latitude == 53.901m && x.Longitude == 27.561m);
    }

    [Fact]
    public async Task OverpassPlacesProvider_ShouldDeduplicateAndCacheResults()
    {
        var fakeClient = new FakeOverpassApiClient
        {
            Result =
            [
                new OverpassElement
                {
                    Id = 101,
                    Type = "node",
                    Latitude = 53.9m,
                    Longitude = 27.56m,
                    Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["shop"] = "supermarket",
                        ["name"] = "Nearby market"
                    }
                },
                new OverpassElement
                {
                    Id = 102,
                    Type = "way",
                    Latitude = 53.9m,
                    Longitude = 27.56m,
                    Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["shop"] = "supermarket",
                        ["name"] = "Nearby market"
                    }
                }
            ]
        };

        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var provider = new OverpassPlacesProvider(
            fakeClient,
            new OverpassPoiMapper(),
            memoryCache,
            Options.Create(new OverpassOptions
            {
                BaseUrl = "https://overpass-api.de/api/interpreter",
                DefaultRadiusMeters = 1500,
                MaxRadiusMeters = 5000,
                RetryCount = 0,
                RetryDelayMs = 1,
                TimeoutSeconds = 25,
                UserAgent = "TestAgent/1.0"
            }),
            Options.Create(new InfrastructureScoringOptions
            {
                CacheMinutes = 30,
                MaxPoiCount = 100,
                MaxRadiusMeters = 5000
            }),
            NullLogger<OverpassPlacesProvider>.Instance);

        var first = await provider.GetNearbyPoisAsync(
            53.9,
            27.56,
            1500,
            [InfrastructureCategory.Commerce],
            100,
            CancellationToken.None);

        var second = await provider.GetNearbyPoisAsync(
            53.9,
            27.56,
            1500,
            [InfrastructureCategory.Commerce],
            100,
            CancellationToken.None);

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(1, fakeClient.CallCount);
        Assert.Equal("osm-overpass", first.Single().Source);
    }

    private sealed class FakeOverpassApiClient : IOverpassApiClient
    {
        public int CallCount { get; private set; }
        public IReadOnlyCollection<OverpassElement> Result { get; init; } = [];

        public Task<IReadOnlyCollection<OverpassElement>> QueryNearbyAsync(
            decimal latitude,
            decimal longitude,
            int radiusMeters,
            IReadOnlyCollection<OverpassTagFilter> filters,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(Result);
        }
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request);
    }
}
