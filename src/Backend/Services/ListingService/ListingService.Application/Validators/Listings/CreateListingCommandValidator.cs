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
            .WithMessage("Необходимо передать данные объявления.");

        RuleFor(x => x.ListingDto.Title)
            .NotEmpty()
            .WithMessage("Заголовок объявления не должен быть пустым.")
            .MaximumLength(TitleMaxLength)
            .WithMessage($"Максимальная длина должна быть не более {TitleMaxLength} символов.");

        RuleFor(x => x.ListingDto.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Максимальная длина должна быть не более {DescriptionMaxLength} символов.");

        RuleFor(x => x.ListingDto.PriceAmount)
            .GreaterThan(0)
            .WithMessage("Цена должна быть больше 0.");

        RuleFor(x => x.ListingDto.PriceAmountByn)
            .GreaterThan(0)
            .When(x => x.ListingDto.PriceAmountByn.HasValue)
            .WithMessage("Цена в BYN должна быть больше 0.");

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
