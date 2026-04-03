using ListingService.Application.UseCases.RenovationTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteRenovationTypeValidator : AbstractValidator<DeleteRenovationTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteRenovationTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа ремонта не должно быть пустым.");
    }
}
