using ListingService.Application.UseCases.LayoutTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class CreateLayoutTypeValidator : AbstractValidator<CreateLayoutTypeCommand>
{
    public const int MaximumLength = 100;

    public CreateLayoutTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа планировки не должно быть пустым.");
    }
}
