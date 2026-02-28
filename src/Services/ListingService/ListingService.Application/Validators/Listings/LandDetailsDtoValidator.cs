using ListingService.Application.Validators.Listings.Extensions;
using ListingService.Application.Contracts.Cache;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentValidation;

namespace ListingService.Application.Validators.Listings;

public class LandDetailsDtoValidator : AbstractValidator<LandDetailsDto>
{
    public LandDetailsDtoValidator(ILookupCache lookupCache)
    {
        RuleFor(x => x.LandCategory)
            .MustExistInLookup<LandDetailsDto, LandCategory>(lookupCache, nameof(LandCategory));

        RuleFor(x => x.RoadAccessType)
            .MustExistInLookup<LandDetailsDto, RoadAccessType>(lookupCache, nameof(RoadAccessType));
    }
}
