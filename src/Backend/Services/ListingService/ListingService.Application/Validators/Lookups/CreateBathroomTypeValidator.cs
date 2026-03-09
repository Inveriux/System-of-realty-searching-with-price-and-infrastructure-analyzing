using ListingService.Application.UseCases.BathroomTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateBathroomTypeValidator : AbstractValidator<CreateBathroomTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateBathroomTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of bathroom type shouldn't be empty.");
    }
}
