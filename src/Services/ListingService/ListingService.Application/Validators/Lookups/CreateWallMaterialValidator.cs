using ListingService.Application.UseCases.WallMaterials;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateWallMaterialValidator : AbstractValidator<CreateWallMaterialCommand>
{
    public const int MaximumLength = 100;

    public CreateWallMaterialValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
