using ListingService.Application.UseCases.ListingTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteListingTypeValidator : AbstractValidator<DeleteListingTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteListingTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название типа объявления не должно быть пустым.");
    }
}
