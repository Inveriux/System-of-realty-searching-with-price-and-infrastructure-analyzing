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
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of building material shouldn't be empty.");
    }
}
