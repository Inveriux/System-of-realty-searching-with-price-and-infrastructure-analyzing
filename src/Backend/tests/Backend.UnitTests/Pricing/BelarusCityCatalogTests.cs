using FluentAssertions;
using PricingAnalyticsService.Services;
using Xunit;

namespace Backend.UnitTests.Pricing;

[Trait("Category", "Unit")]
public sealed class BelarusCityCatalogTests
{
    [Fact]
    public void Cities_ShouldBeDistinctAndContainMinsk()
    {
        BelarusCityCatalog.Cities
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Should()
            .HaveCount(BelarusCityCatalog.Cities.Count);
        BelarusCityCatalog.Cities.Should().Contain(city => city == "Минск");
    }

    [Fact]
    public void Cities_ShouldBeSortedUsingBelarusianRussianCultureRules()
    {
        var expected = BelarusCityCatalog.Cities
            .OrderBy(city => city, StringComparer.Create(new System.Globalization.CultureInfo("ru-BY"), ignoreCase: true))
            .ToArray();

        BelarusCityCatalog.Cities.Should().Equal(expected);
    }
}
