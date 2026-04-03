using FluentValidation;
using InfrastructureService.UseCases.Infrastructure;

namespace InfrastructureService.Validations;

public sealed class RefreshInfrastructureCommandValidator : AbstractValidator<RefreshInfrastructureCommand>
{
    public RefreshInfrastructureCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запрос на обновление инфраструктуры обязателен.");

        RuleFor(x => x)
            .Must(x =>
                x.Request.LocationId.HasValue ||
                !string.IsNullOrWhiteSpace(x.Request.AddressText) ||
                (x.Request.Latitude.HasValue && x.Request.Longitude.HasValue))
            .WithMessage("Необходимо передать `locationId`, `addressText` или обе координаты.");

        RuleFor(x => x.Request.SearchRadiusMeters)
            .GreaterThan(0)
            .LessThanOrEqualTo(10000);
    }
}
