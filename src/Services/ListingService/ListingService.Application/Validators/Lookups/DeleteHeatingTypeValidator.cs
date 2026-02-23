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
            .NotEmpty();
    }
}
