using ListingService.Application.UseCases.LandCategories;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteLandCategoryValidator : AbstractValidator<DeleteLandCategoryCommand>
{
    public const int MaximumLength = 100;

    public DeleteLandCategoryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of land category shouldn't be empty.");
    }
}
