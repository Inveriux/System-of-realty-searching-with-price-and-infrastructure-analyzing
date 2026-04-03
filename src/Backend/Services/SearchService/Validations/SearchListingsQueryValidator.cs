using FluentValidation;
using SearchService.DTOs.Search;
using SearchService.UseCases.Search;

namespace SearchService.Validations;

public sealed class SearchListingsQueryValidator : AbstractValidator<SearchListingsQuery>
{
    public SearchListingsQueryValidator()
    {
        RuleFor(x => x.Request.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Request)
            .Must(r =>
            {
                var minPrice = r.PriceMin ?? r.MinPrice;
                var maxPrice = r.PriceMax ?? r.MaxPrice;
                return !minPrice.HasValue || !maxPrice.HasValue || maxPrice.Value >= minPrice.Value;
            })
            .WithMessage("Максимальная цена должна быть больше или равна минимальной.");
        RuleFor(x => x.Request)
            .Must(r =>
            {
                var minArea = r.AreaMin ?? r.MinArea;
                var maxArea = r.AreaMax ?? r.MaxArea;
                return !minArea.HasValue || !maxArea.HasValue || maxArea.Value >= minArea.Value;
            })
            .WithMessage("Максимальная площадь должна быть больше или равна минимальной.");
        RuleFor(x => x.Request.RadiusMeters)
            .GreaterThan(0)
            .When(x => x.Request.RadiusMeters.HasValue);
        RuleFor(x => x.Request)
            .Must(request =>
                (!ResolveLatitude(request).HasValue && !ResolveLongitude(request).HasValue && !request.RadiusMeters.HasValue) ||
                (ResolveLatitude(request).HasValue && ResolveLongitude(request).HasValue && request.RadiusMeters.HasValue))
            .WithMessage("`latitude`, `longitude` и `radiusMeters` должны передаваться вместе.");
        RuleFor(x => x.Request)
            .Must(request =>
                (!request.BoundsMinLatitude.HasValue && !request.BoundsMinLongitude.HasValue &&
                 !request.BoundsMaxLatitude.HasValue && !request.BoundsMaxLongitude.HasValue) ||
                (request.BoundsMinLatitude.HasValue && request.BoundsMinLongitude.HasValue &&
                 request.BoundsMaxLatitude.HasValue && request.BoundsMaxLongitude.HasValue))
            .WithMessage("Все координаты границ должны передаваться вместе.");
        RuleFor(x => x.Request.Sort ?? x.Request.SortBy)
            .Must(sort => sort is null || AllowedSorts.Contains(sort.Trim().ToLowerInvariant()))
            .WithMessage("Неподдерживаемый режим сортировки.");
    }

    private static readonly HashSet<string> AllowedSorts =
    [
        "relevance",
        "price_asc",
        "price_desc",
        "newest",
        "published_desc",
        "area_desc",
        "infrastructure_desc"
    ];

    private static decimal? ResolveLatitude(SearchListingsRequestDto request)
        => request.CenterLatitude ?? request.Latitude;

    private static decimal? ResolveLongitude(SearchListingsRequestDto request)
        => request.CenterLongitude ?? request.Longitude;
}
