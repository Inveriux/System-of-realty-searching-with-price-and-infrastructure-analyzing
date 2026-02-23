using ListingService.Application.UseCases.WaterSupplyTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateWaterSupplyTypeValidator : AbstractValidator<CreateWaterSupplyTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateWaterSupplyTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
