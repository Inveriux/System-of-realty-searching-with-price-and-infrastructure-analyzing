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
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа здания не должно быть пустым.");
    }
}
