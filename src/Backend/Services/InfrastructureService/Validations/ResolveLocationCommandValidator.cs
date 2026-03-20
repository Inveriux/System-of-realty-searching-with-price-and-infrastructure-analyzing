using FluentValidation;
using InfrastructureService.UseCases.Locations;

namespace InfrastructureService.Validations;

public sealed class ResolveLocationCommandValidator : AbstractValidator<ResolveLocationCommand>
{
    public ResolveLocationCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Location resolve request is required.");

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Request.AddressText) ||
                (x.Request.Latitude.HasValue && x.Request.Longitude.HasValue))
            .WithMessage("AddressText or both coordinates are required.");
    }
}
