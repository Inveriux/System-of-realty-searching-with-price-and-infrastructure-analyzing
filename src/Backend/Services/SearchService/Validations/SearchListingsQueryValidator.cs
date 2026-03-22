using FluentValidation;
using SearchService.UseCases.Search;

namespace SearchService.Validations;

public sealed class SearchListingsQueryValidator : AbstractValidator<SearchListingsQuery>
{
    public SearchListingsQueryValidator()
    {
        RuleFor(x => x.Request.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Request.MaxPrice)
            .GreaterThanOrEqualTo(x => x.Request.MinPrice!.Value)
            .When(x => x.Request.MinPrice.HasValue && x.Request.MaxPrice.HasValue);
        RuleFor(x => x.Request.MaxArea)
            .GreaterThanOrEqualTo(x => x.Request.MinArea!.Value)
            .When(x => x.Request.MinArea.HasValue && x.Request.MaxArea.HasValue);
        RuleFor(x => x.Request.RadiusMeters)
            .GreaterThan(0)
            .When(x => x.Request.RadiusMeters.HasValue);
        RuleFor(x => x.Request)
            .Must(request =>
                (!request.CenterLatitude.HasValue && !request.CenterLongitude.HasValue && !request.RadiusMeters.HasValue) ||
                (request.CenterLatitude.HasValue && request.CenterLongitude.HasValue && request.RadiusMeters.HasValue))
            .WithMessage("CenterLatitude, CenterLongitude and RadiusMeters must be passed together.");
        RuleFor(x => x.Request)
            .Must(request =>
                (!request.BoundsMinLatitude.HasValue && !request.BoundsMinLongitude.HasValue &&
                 !request.BoundsMaxLatitude.HasValue && !request.BoundsMaxLongitude.HasValue) ||
                (request.BoundsMinLatitude.HasValue && request.BoundsMinLongitude.HasValue &&
                 request.BoundsMaxLatitude.HasValue && request.BoundsMaxLongitude.HasValue))
            .WithMessage("All bounds coordinates must be passed together.");
    }
}
