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
            .NotEmpty();
    }
}
