using ListingService.Application.UseCases.LayoutTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteLayoutTypeValidator : AbstractValidator<DeleteLayoutTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteLayoutTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
