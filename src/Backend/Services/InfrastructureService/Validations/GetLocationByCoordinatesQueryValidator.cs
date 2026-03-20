using FluentValidation;
using InfrastructureService.UseCases.Locations;

namespace InfrastructureService.Validations;

public sealed class GetLocationByCoordinatesQueryValidator : AbstractValidator<GetLocationByCoordinatesQuery>
{
    public GetLocationByCoordinatesQueryValidator()
    {
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
    }
}
