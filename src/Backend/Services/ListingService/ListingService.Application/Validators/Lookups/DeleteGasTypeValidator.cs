using ListingService.Application.UseCases.GasTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteGasTypeValidator : AbstractValidator<DeleteGasTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteGasTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа газа не должно быть пустым.");
    }
}
