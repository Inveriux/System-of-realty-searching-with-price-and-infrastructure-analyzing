using ListingService.Application.UseCases.RoadAccessTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateRoadAccessTypeValidator : AbstractValidator<CreateRoadAccessTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateRoadAccessTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа подъезда к дороге не должно быть пустым.");
    }
}
