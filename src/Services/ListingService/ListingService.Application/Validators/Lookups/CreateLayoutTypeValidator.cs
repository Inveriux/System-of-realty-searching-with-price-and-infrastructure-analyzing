using ListingService.Application.UseCases.LayoutTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateLayoutTypeValidator : AbstractValidator<CreateLayoutTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateLayoutTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of layout type shouldn't be empty.");
    }
}
