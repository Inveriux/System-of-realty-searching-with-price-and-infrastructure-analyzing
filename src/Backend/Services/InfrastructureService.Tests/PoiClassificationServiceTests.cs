using InfrastructureService.Services.Classification;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Places.Abstractions;
using Xunit;

namespace InfrastructureService.Tests;

public sealed class PoiClassificationServiceTests
{
    private readonly PoiClassificationService _service = new(new PoiClassificationDictionaryProvider(), new PlaceCategoryNormalizer());

    [Fact]
    public void ProviderTags_ShouldOverrideNameKeyword()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Аптека Green",
            Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["shop"] = "supermarket"
            }
        });

        Assert.NotNull(result);
        Assert.Equal("commerce", result!.CategoryCode);
        Assert.Equal("supermarket", result.SubcategoryCode);
        Assert.Equal("provider-tags", result.Source);
    }

    [Fact]
    public void RawCategories_ShouldResolveMetroWithoutFallback()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Октябрьская",
            RawCategories = ["public_transport.metro"]
        });

        Assert.NotNull(result);
        Assert.Equal("transport", result!.CategoryCode);
        Assert.Equal("metro_station", result.SubcategoryCode);
    }

    [Fact]
    public void ProviderTags_ShouldResolveMetroPlatformWithSubwayFlag()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Площадь Ленина",
            Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["public_transport"] = "platform",
                ["subway"] = "yes"
            }
        });

        Assert.NotNull(result);
        Assert.Equal("transport", result!.CategoryCode);
        Assert.Equal("metro_station", result.SubcategoryCode);
        Assert.Equal("provider-tags", result.Source);
    }

    [Fact]
    public void BrandDictionary_ShouldClassifyKnownRetailBrand()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Евроопт на Сурганова"
        });

        Assert.NotNull(result);
        Assert.Equal("commerce", result!.CategoryCode);
        Assert.Equal("supermarket", result.SubcategoryCode);
        Assert.Equal("brand-dictionary", result.Source);
    }

    [Fact]
    public void StopKeywordDictionary_ShouldNotConfuseBusStopWithMetro()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Автобусная остановка Площадь Победы",
            RawCategories = ["public_transport"],
            Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["public_transport"] = "platform"
            }
        });

        Assert.NotNull(result);
        Assert.Equal("transport", result!.CategoryCode);
        Assert.Equal("bus_stop", result.SubcategoryCode);
    }

    [Fact]
    public void ExcludeKeyword_ShouldFilterReligiousPlace()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Церковь Святого Духа"
        });

        Assert.Null(result);
    }

    [Fact]
    public void GeneralCategoryFallback_ShouldReturnCategoryWithoutSubcategory()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Business block A",
            RawCategory = "commercial"
        });

        Assert.NotNull(result);
        Assert.Equal("commerce", result!.CategoryCode);
        Assert.Null(result.SubcategoryCode);
        Assert.Equal("category-fallback", result.Source);
    }

    [Fact]
    public void ProviderTags_ShouldClassifyPark()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Парк Челюскинцев",
            Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["leisure"] = "park"
            }
        });

        Assert.NotNull(result);
        Assert.Equal("leisure", result!.CategoryCode);
        Assert.Equal("park", result.SubcategoryCode);
    }

    [Theory]
    [InlineData("World Class", "fitness")]
    [InlineData("Национальный художественный музей", "museum")]
    [InlineData("Большой театр Беларуси", "theater")]
    [InlineData("Silver Screen", "cinema")]
    public void LeisureKeywords_AndBrands_ShouldResolveExpectedSubcategory(string name, string expectedSubcategory)
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = name
        });

        Assert.NotNull(result);
        Assert.Equal("leisure", result!.CategoryCode);
        Assert.Equal(expectedSubcategory, result.SubcategoryCode);
    }

    [Fact]
    public void ProviderTags_ShouldTakePriorityOverLeisureBrandAliases()
    {
        var result = _service.Classify(new ExternalPoiResult
        {
            Name = "Silver Screen Arena City",
            Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["amenity"] = "restaurant"
            }
        });

        Assert.NotNull(result);
        Assert.Equal("leisure", result!.CategoryCode);
        Assert.Equal("restaurant", result.SubcategoryCode);
        Assert.Equal("provider-tags", result.Source);
    }
}
