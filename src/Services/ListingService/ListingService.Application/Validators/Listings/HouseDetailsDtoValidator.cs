using ListingService.Application.Validators.Listings.Extensions;
using ListingService.Application.Contracts.Cache;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentValidation;

namespace ListingService.Application.Validators.Listings;

public class HouseDetailsDtoValidator : AbstractValidator<HouseDetailsDto>
{
    public HouseDetailsDtoValidator(ILookupCache lookupCache)
    {
        RuleFor(x => x.WallMaterial)
            .MustExistInLookup<HouseDetailsDto, WallMaterial>(lookupCache, nameof(WallMaterial));

        RuleFor(x => x.RoofMaterial)
            .MustExistInLookup<HouseDetailsDto, WallMaterial>(lookupCache, nameof(WallMaterial));

        RuleFor(x => x.HeatingType)
            .MustExistInLookup<HouseDetailsDto, HeatingType>(lookupCache, nameof(HeatingType));

        RuleFor(x => x.WaterSupplyType)
            .MustExistInLookup<HouseDetailsDto, WaterSupplyType>(lookupCache, nameof(WaterSupplyType));

        RuleFor(x => x.SewerType)
            .MustExistInLookup<HouseDetailsDto, SewerType>(lookupCache, nameof(SewerType));

        RuleFor(x => x.GasType)
            .MustExistInLookup<HouseDetailsDto, GasType>(lookupCache, nameof(GasType));
    }
}
