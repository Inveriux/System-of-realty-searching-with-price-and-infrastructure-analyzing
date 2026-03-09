using ListingService.Application.UseCases.RoadAccessTypes;
using FluentValidation;

namespace ListingService.Application.Validators.Lookups;

public class DeleteRoadAccessTypeValidator : AbstractValidator<DeleteRoadAccessTypeCommand>
{
    public const int MaximumLength = 100;

    public DeleteRoadAccessTypeValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(MaximumLength)
            .WithMessage("Maximum length must be equals or less than 100 characters.")
            .NotEmpty()
            .WithMessage("Name of road access type shouldn't be empty.");
    }
}
