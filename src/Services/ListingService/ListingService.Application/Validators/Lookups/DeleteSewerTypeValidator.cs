using ListingService.Application.UseCases.SewerTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteSewerTypeValidator : AbstractValidator<DeleteSewerTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteSewerTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of sewer type shouldn't be empty.");
    }
}
