using ListingService.Application.Validators.Listings.Extensions;
using ListingService.Application.Contracts.Cache;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentValidation;

namespace ListingService.Application.Validators.Listings;

public class WarehouseDetailsDtoValidator : AbstractValidator<WarehouseDetailsDto>
{
    public WarehouseDetailsDtoValidator(ILookupCache lookupCache)
    {
        RuleFor(x => x.ClassType)
            .MustExistInLookup<WarehouseDetailsDto, ClassType>(lookupCache, nameof(ClassType));

        RuleFor(x => x.HeatingType)
            .MustExistInLookup<WarehouseDetailsDto, HeatingType>(lookupCache, nameof(HeatingType));
    }
}
