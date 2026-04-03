using ListingService.Application.UseCases.GasTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateGasTypeValidator : AbstractValidator<CreateGasTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateGasTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа газа не должно быть пустым.");
    }
}
