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
            .NotEmpty();
    }
}
