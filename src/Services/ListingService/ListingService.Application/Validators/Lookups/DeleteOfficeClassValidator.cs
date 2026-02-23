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
            .NotEmpty();
    }
}
