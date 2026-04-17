using FluentAssertions;
using SearchService.DTOs.Search;
using SearchService.UseCases.Search;
using SearchService.Validations;
using Xunit;

namespace Backend.UnitTests.Search;

[Trait("Category", "Unit")]
public sealed class SearchListingsQueryValidatorTests
{
    private readonly SearchListingsQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenPriceRangeIsInverted_ShouldReportValidationError()
    {
        var query = new SearchListingsQuery(new SearchListingsRequestDto
        {
            PriceMin = 120_000m,
            PriceMax = 95_000m,
            PageSize = 10
        });

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.ErrorMessage.Contains("Максимальная цена", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WhenRadiusIsProvidedWithoutCoordinates_ShouldReportValidationError()
    {
        var query = new SearchListingsQuery(new SearchListingsRequestDto
        {
            RadiusMeters = 500m,
            Latitude = 53.9m,
            PageSize = 10
        });

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.ErrorMessage.Contains("radiusMeters", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WhenGeoSearchPayloadIsComplete_ShouldPass()
    {
        var query = new SearchListingsQuery(new SearchListingsRequestDto
        {
            CenterLatitude = 53.9m,
            CenterLongitude = 27.56m,
            RadiusMeters = 800m,
            Sort = "relevance",
            Page = 0,
            PageSize = 20
        });

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
