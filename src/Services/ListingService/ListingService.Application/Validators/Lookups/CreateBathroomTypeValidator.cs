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
            .NotEmpty();
    }
}
