using ListingService.Application.UseCases.OfficeClasses;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteOfficeClassValidator : AbstractValidator<DeleteOfficeClassCommand>
{
    public const int MaximumLength = 100;

    public DeleteOfficeClassValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Максимальная длина должна быть не более 100 символов.")
            .NotEmpty()
            .WithMessage("Название офисного класса не должно быть пустым.");
    }
}
