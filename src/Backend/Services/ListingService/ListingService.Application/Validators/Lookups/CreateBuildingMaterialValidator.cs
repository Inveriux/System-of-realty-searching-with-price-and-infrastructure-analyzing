using ListingService.Application.UseCases.BuildingMaterials;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateBuildingMaterialValidator : AbstractValidator<CreateBuildingMaterialCommand>
{
    public const int MaximumLength = 100;

    public CreateBuildingMaterialValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название материала здания не должно быть пустым.");
    }
}
