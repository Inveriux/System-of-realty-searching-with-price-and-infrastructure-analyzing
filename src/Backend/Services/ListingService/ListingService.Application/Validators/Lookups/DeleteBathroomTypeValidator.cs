using ListingService.Application.UseCases.BathroomTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteBathroomTypeValidator : AbstractValidator<DeleteBathroomTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteBathroomTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа санузла не должно быть пустым.");
    }
}
