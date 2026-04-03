using ListingService.Application.UseCases.SewerTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteSewerTypeValidator : AbstractValidator<DeleteSewerTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteSewerTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа канализации не должно быть пустым.");
    }
}
