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
            .MustExistInLookupRequired<HouseDetailsDto, WallMaterial>(lookupCache, nameof(WallMaterial));

        RuleFor(x => x.RoofMaterial)
            .MustExistInLookupRequired<HouseDetailsDto, WallMaterial>(lookupCache, nameof(WallMaterial));

        RuleFor(x => x.HeatingType)
            .MustExistInLookupRequired<HouseDetailsDto, HeatingType>(lookupCache, nameof(HeatingType));

        RuleFor(x => x.WaterSupplyType)
            .MustExistInLookupRequired<HouseDetailsDto, WaterSupplyType>(lookupCache, nameof(WaterSupplyType));

        RuleFor(x => x.SewerType)
            .MustExistInLookupRequired<HouseDetailsDto, SewerType>(lookupCache, nameof(SewerType));

        RuleFor(x => x.GasType)
            .MustExistInLookupRequired<HouseDetailsDto, GasType>(lookupCache, nameof(GasType));
    }
}
