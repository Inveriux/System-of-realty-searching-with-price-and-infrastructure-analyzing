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
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа водоснабжения не должно быть пустым.");
    }
}
