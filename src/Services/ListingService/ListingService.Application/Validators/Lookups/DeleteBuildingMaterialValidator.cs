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
            .NotEmpty();
    }
}
