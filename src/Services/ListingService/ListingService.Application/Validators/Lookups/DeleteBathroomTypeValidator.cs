using ListingService.Application.UseCases.BathroomTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteBathroomTypeValidator : AbstractValidator<DeleteBathroomTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteBathroomTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
