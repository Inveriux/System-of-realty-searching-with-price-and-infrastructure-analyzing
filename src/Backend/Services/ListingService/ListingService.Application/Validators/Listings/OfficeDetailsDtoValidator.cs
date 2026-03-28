using ListingService.Application.Validators.Listings.Extensions;
using ListingService.Application.Contracts.Cache;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentValidation;

namespace ListingService.Application.Validators.Listings;

public class OfficeDetailsDtoValidator : AbstractValidator<OfficeDetailsDto>
{
    public OfficeDetailsDtoValidator(ILookupCache lookupCache)
    {
        RuleFor(x => x.OfficeClass)
            .MustExistInLookup<OfficeDetailsDto, OfficeClass>(lookupCache, nameof(OfficeClass));

        RuleFor(x => x.LayoutType)
            .MustExistInLookup<OfficeDetailsDto, LayoutType>(lookupCache, nameof(LayoutType));

        RuleFor(x => x.RenovationType)
            .MustExistInLookup<OfficeDetailsDto, RenovationType>(lookupCache, nameof(RenovationType));

        RuleFor(x => x.ParkingType)
            .MustExistInLookup<OfficeDetailsDto, LayoutType>(
                lookupCache,
                nameof(LayoutType),
                nameof(OfficeDetailsDto.ParkingType));
    }
}
