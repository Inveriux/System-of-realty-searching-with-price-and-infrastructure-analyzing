using ListingService.Application.UseCases.BuildingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteBuildingTypeValidator : AbstractValidator<DeleteBuildingTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteBuildingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
