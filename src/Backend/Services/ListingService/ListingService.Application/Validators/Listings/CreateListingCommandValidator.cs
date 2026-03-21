using ListingService.Application.UseCases.Listings;
using ListingService.Application.DTOs.Listings;
using FluentValidation;

namespace ListingService.Application.Validators.Listings;

public class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
{
    private const int TitleMaxLength = 100;
    private const int DescriptionMaxLength = 1000;


    public CreateListingCommandValidator(IValidator<ApartmentDetailsDto> apartmentDetailsValidator,
        IValidator<HouseDetailsDto> houseDetailsValidator,
        IValidator<LandDetailsDto> landDetailsValidator,
        IValidator<WarehouseDetailsDto> warehouseDetailsValidator,
        IValidator<OfficeDetailsDto> officeDetailsValidator)
    {
        RuleFor(x => x.ListingDto)
            .NotNull()
            .WithMessage("Listing data must be provided.");

        RuleFor(x => x.ListingDto.Title)
            .NotEmpty()
            .WithMessage("Title of listing shouldn't be empty.")
            .MaximumLength(TitleMaxLength)
            .WithMessage($"Maximum length must be equals or less than {TitleMaxLength} characters.");

        RuleFor(x => x.ListingDto.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Maximum length must be equals or less than {DescriptionMaxLength} characters.");

        RuleFor(x => x.ListingDto.PriceAmount)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.ListingDto.PriceAmountByn)
            .GreaterThan(0)
            .When(x => x.ListingDto.PriceAmountByn.HasValue)
            .WithMessage("Price in BYN must be greater than 0.");

        When(x => x.ListingDto.ListingDetails is ApartmentDetailsDto, () =>
        {
            RuleFor(x => (ApartmentDetailsDto)x.ListingDto.ListingDetails)
                .SetValidator(apartmentDetailsValidator);
        });

        When(x => x.ListingDto.ListingDetails is HouseDetailsDto, () =>
        {
            RuleFor(x => (HouseDetailsDto)x.ListingDto.ListingDetails)
                .SetValidator(houseDetailsValidator);
        });

        When(x => x.ListingDto.ListingDetails is LandDetailsDto, () =>
        {
            RuleFor(x => (LandDetailsDto)x.ListingDto.ListingDetails)
                .SetValidator(landDetailsValidator);
        });

        When(x => x.ListingDto.ListingDetails is WarehouseDetailsDto, () =>
        {
            RuleFor(x => (WarehouseDetailsDto)x.ListingDto.ListingDetails)
                .SetValidator(warehouseDetailsValidator);
        });

        When(x => x.ListingDto.ListingDetails is OfficeDetailsDto, () =>
        {
            RuleFor(x => (OfficeDetailsDto)x.ListingDto.ListingDetails)
                .SetValidator(officeDetailsValidator);
        });
    }
}
