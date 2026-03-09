using ListingService.Application.Validators.Listings.Extensions;
using ListingService.Application.Contracts.Cache;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentValidation;

namespace ListingService.Application.Validators.Listings;

public class ApartmentDetailsDtoValidator : AbstractValidator<ApartmentDetailsDto>
{
    public ApartmentDetailsDtoValidator(ILookupCache lookupCache)
    {
        RuleFor(x => x.BathroomType)
            .MustExistInLookup<ApartmentDetailsDto, BathroomType>(lookupCache, 
                                                                  nameof(BathroomType));

        RuleFor(x => x.RenovationType)
            .MustExistInLookup<ApartmentDetailsDto, RenovationType>(lookupCache, 
                                                                    nameof(RenovationType));

        RuleFor(x => x.BuildingMaterial)
            .MustExistInLookup<ApartmentDetailsDto, BuildingMaterial>(lookupCache, 
                                                                      nameof(BuildingMaterial));
    }
}
