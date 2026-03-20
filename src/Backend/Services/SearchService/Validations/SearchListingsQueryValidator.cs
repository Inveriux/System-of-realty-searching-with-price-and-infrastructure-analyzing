using FluentValidation;
using SearchService.UseCases.Search;

namespace SearchService.Validations;

public sealed class SearchListingsQueryValidator : AbstractValidator<SearchListingsQuery>
{
    public SearchListingsQueryValidator()
    {
        RuleFor(x => x.Request.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.PageSize).InclusiveBetween(1, 100);
    }
}
