using ListingService.Application.UseCases.SewerTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateSewerTypeValidator : AbstractValidator<CreateSewerTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateSewerTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
