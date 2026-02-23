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
            .NotEmpty();
    }
}
