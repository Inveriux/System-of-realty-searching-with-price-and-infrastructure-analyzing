using ListingService.Application.UseCases.WaterSupplyTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteWaterSupplyTypeValidator : AbstractValidator<DeleteWaterSupplyTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteWaterSupplyTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
