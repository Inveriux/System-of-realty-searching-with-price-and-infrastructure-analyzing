using ListingService.Application.UseCases.HeatingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateHeatingTypeValidator : AbstractValidator<CreateHeatingTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateHeatingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа отопления не должно быть пустым.");
    }
}
