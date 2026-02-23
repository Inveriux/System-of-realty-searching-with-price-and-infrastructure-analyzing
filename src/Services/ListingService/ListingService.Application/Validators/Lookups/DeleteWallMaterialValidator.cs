using ListingService.Application.UseCases.WallMaterials;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteWallMaterialValidator : AbstractValidator<DeleteWallMaterialCommand>
{
    public const int MaximumLength = 100;

    public DeleteWallMaterialValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
