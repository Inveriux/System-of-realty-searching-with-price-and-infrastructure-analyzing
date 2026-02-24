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
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of gas type shouldn't be empty.");
    }
}
