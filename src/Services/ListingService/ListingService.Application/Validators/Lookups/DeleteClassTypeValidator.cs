using ListingService.Application.UseCases.ClassTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteClassTypeValidator : AbstractValidator<DeleteClassTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteClassTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of class type shouldn't be empty.");
    }
}
