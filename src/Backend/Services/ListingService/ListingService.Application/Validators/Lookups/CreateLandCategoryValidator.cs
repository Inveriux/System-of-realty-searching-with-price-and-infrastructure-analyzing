using ListingService.Application.UseCases.LandCategories;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateLandCategoryValidator : AbstractValidator<CreateLandCategoryCommand>
{
    public const int MaximumLength = 100;

    public CreateLandCategoryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название категории земли не должно быть пустым.");
    }
}
