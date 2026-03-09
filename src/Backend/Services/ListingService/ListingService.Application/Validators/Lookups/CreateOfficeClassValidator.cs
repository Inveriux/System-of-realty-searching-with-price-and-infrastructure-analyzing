using ListingService.Application.UseCases.OfficeClasses;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateOfficeClassValidator : AbstractValidator<CreateOfficeClassCommand>
{
    public const int MaximumLength = 100;

    public CreateOfficeClassValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of office class shouldn't be empty.");
    }
}
