using ListingService.Application.UseCases.RenovationTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateRenovationTypeValidator : AbstractValidator<CreateRenovationTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateRenovationTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of renovation type shouldn't be empty.");
    }
}
