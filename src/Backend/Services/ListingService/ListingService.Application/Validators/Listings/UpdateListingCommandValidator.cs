using FluentValidation;
using ListingService.Application.DTOs.Listings;
using ListingService.Application.UseCases.Listings;

namespace ListingService.Application.Validators.Listings;

public sealed class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
{
    private const int TitleMaxLength = 100;
    private const int DescriptionMaxLength = 1000;

    public UpdateListingCommandValidator(
        IValidator<ApartmentDetailsDto> apartmentDetailsValidator,
        IValidator<HouseDetailsDto> houseDetailsValidator,
        IValidator<LandDetailsDto> landDetailsValidator,
        IValidator<WarehouseDetailsDto> warehouseDetailsValidator,
        IValidator<OfficeDetailsDto> officeDetailsValidator)
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Необходимо передать данные объявления.");

        RuleFor(x => x.Request.Title)
            .NotEmpty()
            .WithMessage("Заголовок объявления не должен быть пустым.")
            .MaximumLength(TitleMaxLength)
            .WithMessage($"Максимальная длина должна быть не более {TitleMaxLength} символов.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Максимальная длина должна быть не более {DescriptionMaxLength} символов.");

        RuleFor(x => x.Request.PriceAmount)
            .GreaterThan(0)
            .When(x => x.Request.PriceAmount.HasValue)
            .WithMessage("Цена должна быть больше 0.");

        RuleFor(x => x.Request.PriceAmountByn)
            .GreaterThan(0)
            .When(x => x.Request.PriceAmountByn.HasValue)
            .WithMessage("Цена в BYN должна быть больше 0.");

        When(x => x.Request.ListingDetails is ApartmentDetailsDto, () =>
        {
            RuleFor(x => (ApartmentDetailsDto)x.Request.ListingDetails)
                .SetValidator(apartmentDetailsValidator);
        });

        When(x => x.Request.ListingDetails is HouseDetailsDto, () =>
        {
            RuleFor(x => (HouseDetailsDto)x.Request.ListingDetails)
                .SetValidator(houseDetailsValidator);
        });

        When(x => x.Request.ListingDetails is LandDetailsDto, () =>
        {
            RuleFor(x => (LandDetailsDto)x.Request.ListingDetails)
                .SetValidator(landDetailsValidator);
        });

        When(x => x.Request.ListingDetails is WarehouseDetailsDto, () =>
        {
            RuleFor(x => (WarehouseDetailsDto)x.Request.ListingDetails)
                .SetValidator(warehouseDetailsValidator);
        });

        When(x => x.Request.ListingDetails is OfficeDetailsDto, () =>
        {
            RuleFor(x => (OfficeDetailsDto)x.Request.ListingDetails)
                .SetValidator(officeDetailsValidator);
        });
    }
}
