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
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название категории земли не должно быть пустым.");
    }
}
