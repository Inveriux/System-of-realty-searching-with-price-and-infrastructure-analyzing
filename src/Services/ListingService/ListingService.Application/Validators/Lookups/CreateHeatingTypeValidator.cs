using ListingService.Application.UseCases.HeatingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateHeatingTypeValidator : AbstractValidator<CreateHeatingTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateHeatingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
