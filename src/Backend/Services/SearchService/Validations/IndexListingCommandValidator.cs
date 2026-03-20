using FluentValidation;
using SearchService.UseCases.Indexing;

namespace SearchService.Validations;

public sealed class IndexListingCommandValidator : AbstractValidator<IndexListingCommand>
{
    public IndexListingCommandValidator()
    {
        RuleFor(x => x.Request.ListingId).NotEmpty();
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.AreaTotal).GreaterThanOrEqualTo(0);
    }
}
