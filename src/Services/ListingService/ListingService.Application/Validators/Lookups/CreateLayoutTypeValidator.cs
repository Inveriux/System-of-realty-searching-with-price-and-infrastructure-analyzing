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
            .NotEmpty();
    }
}
