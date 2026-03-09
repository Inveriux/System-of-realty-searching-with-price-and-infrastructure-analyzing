using ListingService.Application.UseCases.ClassTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateClassTypeValidator : AbstractValidator<CreateClassTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateClassTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of class type shouldn't be empty.");
    }
}
