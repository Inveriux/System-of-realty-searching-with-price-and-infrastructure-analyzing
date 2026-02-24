using ListingService.Application.UseCases.OfficeClasses;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteOfficeClassValidator : AbstractValidator<DeleteOfficeClassCommand>
{
    public const int MaximumLength = 100;

    public DeleteOfficeClassValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of office class shouldn't be empty.");
    }
}
