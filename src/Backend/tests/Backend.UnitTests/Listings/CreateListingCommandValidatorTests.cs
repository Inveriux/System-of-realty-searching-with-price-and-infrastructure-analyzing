using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using ListingService.Application.DTOs.Listings;
using ListingService.Application.UseCases.Listings;
using ListingService.Application.Validators.Listings;
using Xunit;

namespace Backend.UnitTests.Listings;

[Trait("Category", "Unit")]
public sealed class CreateListingCommandValidatorTests
{
    [Fact]
    public void Validate_WhenTitleIsEmptyAndPriceIsInvalid_ShouldReturnExpectedErrors()
    {
        var validator = CreateValidator();
        var command = new CreateListingCommand(new CreateListingDto
        {
            AddressText = "Минск, ул. Тестовая, 1",
            Title = string.Empty,
            ListingType = "Sale",
            ListingDetailsType = "Apartment",
            PriceAmount = 0m,
            AreaTotal = 55m,
            ListingDetails = new ApartmentDetailsDto()
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "ListingDto.Title");
        result.Errors.Should().Contain(error => error.PropertyName == "ListingDto.PriceAmount");
    }

    [Fact]
    public void Validate_WhenApartmentDetailsValidatorFails_ShouldBubbleNestedValidationError()
    {
        var apartmentValidator = new InlineValidator<ApartmentDetailsDto>();
        apartmentValidator.RuleFor(x => x.Rooms).GreaterThan(0).WithMessage("Rooms must be greater than zero.");

        var validator = CreateValidator(apartmentValidator: apartmentValidator);
        var command = new CreateListingCommand(new CreateListingDto
        {
            AddressText = "Минск, ул. Тестовая, 2",
            Title = "Квартира у парка",
            ListingType = "Sale",
            ListingDetailsType = "Apartment",
            PriceAmount = 120_000m,
            AreaTotal = 60m,
            ListingDetails = new ApartmentDetailsDto
            {
                Rooms = 0
            }
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "Rooms must be greater than zero.");
    }

    private static CreateListingCommandValidator CreateValidator(
        IValidator<ApartmentDetailsDto>? apartmentValidator = null,
        IValidator<HouseDetailsDto>? houseValidator = null,
        IValidator<LandDetailsDto>? landValidator = null,
        IValidator<WarehouseDetailsDto>? warehouseValidator = null,
        IValidator<OfficeDetailsDto>? officeValidator = null)
        => new(
            apartmentValidator ?? new InlineValidator<ApartmentDetailsDto>(),
            houseValidator ?? new InlineValidator<HouseDetailsDto>(),
            landValidator ?? new InlineValidator<LandDetailsDto>(),
            warehouseValidator ?? new InlineValidator<WarehouseDetailsDto>(),
            officeValidator ?? new InlineValidator<OfficeDetailsDto>());
}
