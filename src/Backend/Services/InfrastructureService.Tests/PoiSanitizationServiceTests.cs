using InfrastructureService.Services.Classification;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Sanitization;
using Xunit;

namespace InfrastructureService.Tests;

public sealed class PoiSanitizationServiceTests
{
    private readonly PoiSanitizationService _service = new(new PoiClassificationDictionaryProvider());

    [Fact]
    public void AddressLikeGarbagePoi_ShouldBeExcluded()
    {
        var result = _service.Evaluate(new ExternalPoiResult
        {
            Name = "Беларусь, 220018 Минск, Запад, 2-й переулок Тимошенко",
            RawCategory = "address"
        });

        Assert.True(result.ShouldExclude);
        Assert.Equal("address_like_without_poi_semantics", result.Reason);
    }

    [Fact]
    public void AddressLikeStringWithStrongPoiTags_ShouldBeKept()
    {
        var result = _service.Evaluate(new ExternalPoiResult
        {
            Name = "Green, улица Притыцкого",
            RawCategory = "shop.supermarket",
            Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["shop"] = "supermarket"
            }
        });

        Assert.False(result.ShouldExclude);
        Assert.True(result.PoiConfidenceScore > result.AddressLikenessScore || result.PoiConfidenceScore >= 0.55m);
    }
}
