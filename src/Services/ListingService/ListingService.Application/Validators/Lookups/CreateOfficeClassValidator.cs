using ListingService.Application.UseCases.OfficeClasses;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateOfficeClassValidator : AbstractValidator<CreateOfficeClassCommand>
{
    public const int MaximumLength = 100;

    public CreateOfficeClassValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
