using ListingService.Application.UseCases.HeatingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteHeatingTypeValidator : AbstractValidator<DeleteHeatingTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteHeatingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of heating type shouldn't be empty.");
    }
}
