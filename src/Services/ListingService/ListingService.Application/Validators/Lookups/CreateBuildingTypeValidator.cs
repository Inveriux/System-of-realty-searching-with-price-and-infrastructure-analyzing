using ListingService.Application.UseCases.BuildingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateBuildingTypeValidator : AbstractValidator<CreateBuildingTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateBuildingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .NotEmpty();
    }
}
