using ListingService.Application.UseCases.BuildingMaterials;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteBuildingMaterialValidator : AbstractValidator<DeleteBuildingMaterialCommand>
{
    public const int MaximumLength = 100;

    public DeleteBuildingMaterialValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название материала здания не должно быть пустым.");
    }
}
