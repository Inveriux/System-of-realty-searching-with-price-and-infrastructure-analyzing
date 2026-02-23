using ListingService.Application.UseCases.ListingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateListingTypeValidator : AbstractValidator<CreateListingTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateListingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
