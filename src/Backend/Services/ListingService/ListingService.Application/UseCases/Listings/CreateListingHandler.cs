using ListingService.Application.Contracts.Cache;
using ListingService.Application.Contracts.Services;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Abstractions;
using ListingService.Domain.Enums;
using ListingService.Domain.Models;
using BasicBuildingBlocks.Services.Abstractions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ListingService.Application.UseCases.Listings;

public record CreateListingCommand(CreateListingDto ListingDto) : IRequest<Result>;

public class CreateListingHandler(IRepositoryManager manager, 
    ILookupCache cache, 
    IMapper mapper,
    ICurrentUserProvider currentUserProvider,
    IInfrastructureServiceClient infrastructureServiceClient,
    ICurrencyRateService currencyRateService,
    ISearchServiceClient searchServiceClient,
    ILogger<CreateListingHandler> logger)
    : IRequestHandler<CreateListingCommand, Result>
{
    public async Task<Result> Handle(CreateListingCommand request, 
        CancellationToken cancellationToken)
    {   
        var listing = mapper.Map<Listing>(request.ListingDto);
        listing.AgentId = currentUserProvider.GetUserId();
        await NormalizePriceAsync(listing, request.ListingDto, cancellationToken);

        if (!listing.LocationId.HasValue && !string.IsNullOrWhiteSpace(listing.AddressText))
        {
            var resolvedLocation = await infrastructureServiceClient.ResolveLocationAsync(listing.AddressText, cancellationToken);
            if (resolvedLocation is not null)
            {
                listing.LocationId = resolvedLocation.LocationId;
                listing.Latitude = resolvedLocation.Latitude;
                listing.Longitude = resolvedLocation.Longitude;
            }
        }

        var resolveLookupIdsResult = await ResolveLookupIds(listing, request.ListingDto, cancellationToken);

        if (!resolveLookupIdsResult.IsSuccess)
            return Result.Fail();

        await manager.Listing.AddAsync(listing, cancellationToken);

        await manager.SaveChangesAsync(cancellationToken);

        if (listing.LocationId.HasValue)
        {
            var refreshed = await infrastructureServiceClient.RefreshInfrastructureAsync(listing.LocationId.Value, cancellationToken);
            if (!refreshed)
            {
                logger.LogWarning(
                    "Listing {ListingId} was created, but infrastructure refresh failed for location {LocationId}.",
                    listing.Id,
                    listing.LocationId.Value);
            }
        }

        var indexed = await searchServiceClient.IndexListingAsync(new SearchListingIndexRequest
        {
            ListingId = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            AddressText = listing.AddressText,
            LocationId = listing.LocationId,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            ListingType = request.ListingDto.ListingType,
            BuildingType = request.ListingDto.BuildingType,
            PriceAmount = listing.PriceAmount,
            PriceCurrency = listing.PriceCurrency,
            AreaTotal = listing.AreaTotal,
            MainMediaUrl = request.ListingDto.MediaUrls?.FirstOrDefault(static url => !string.IsNullOrWhiteSpace(url)),
            PublishedDate = listing.PublishedDate
        }, cancellationToken);

        if (!indexed)
        {
            logger.LogWarning(
                "Listing {ListingId} was created, but search indexing failed.",
                listing.Id);
        }

        return Result.Ok();
    }

    private async Task NormalizePriceAsync(Listing listing, CreateListingDto listingDto, CancellationToken cancellationToken)
    {
        var bynAmount = listingDto.PriceAmountByn ?? listingDto.PriceAmount;
        if (!bynAmount.HasValue)
        {
            listing.PriceAmount = null;
            listing.PriceAmountByn = null;
            listing.PriceAmountUsd = null;
            listing.PriceCurrency = null;
            return;
        }

        var conversion = await currencyRateService.ConvertBynToUsdAsync(bynAmount.Value, cancellationToken);

        listing.PriceAmount = conversion.AmountByn;
        listing.PriceCurrency = Currency.BYN.ToString();
        listing.PriceAmountByn = conversion.AmountByn;
        listing.PriceAmountUsd = conversion.AmountUsd;
        listingDto.PriceAmount = conversion.AmountByn;
        listingDto.PriceCurrency = Currency.BYN.ToString();
        listingDto.PriceAmountByn = conversion.AmountByn;
        listingDto.PriceAmountUsd = conversion.AmountUsd;
    }

    private async Task<Result<Guid>> ResolveLookupIds(Listing listing, 
        CreateListingDto listingDto, 
        CancellationToken cancellationToken)
    {
        if (listingDto.BuildingType != null)
        {
            var buildingTypeId = await ResolveLookupIdAsync<BuildingType>(
                nameof(BuildingType),
                listingDto.BuildingType,
                nameof(CreateListingDto.BuildingType),
                cancellationToken);

            if (!buildingTypeId.IsSuccess)
                return buildingTypeId;

            listing.BuildingTypeId = buildingTypeId.Data;
            listing.BuildingType = null;
        }

        var listingTypeId = await ResolveLookupIdAsync<ListingType>(
            nameof(ListingType),
            listingDto.ListingType,
            nameof(CreateListingDto.ListingType),
            cancellationToken);

        if (!listingTypeId.IsSuccess)
            return listingTypeId;

        listing.ListingTypeId = listingTypeId.Data;

        if(listingDto.ListingDetails is ApartmentDetailsDto apartmentDetailsDto
            && listing.ListingDetails is ApartmentDetails apartmentDetails)
        {
            if (apartmentDetailsDto.BathroomType != null)
            {
                var bathroomTypeId = await ResolveLookupIdAsync<BathroomType>(
                    nameof(BathroomType),
                    apartmentDetailsDto.BathroomType,
                    nameof(ApartmentDetailsDto.BathroomType),
                    cancellationToken);

                if (!bathroomTypeId.IsSuccess)
                    return bathroomTypeId;

                apartmentDetails.BathroomTypeId = bathroomTypeId.Data;
                apartmentDetails.BathroomType = null;
            }

            if (apartmentDetailsDto.RenovationType != null)
            {
                var renovationTypeId = await ResolveLookupIdAsync<RenovationType>(
                    nameof(RenovationType),
                    apartmentDetailsDto.RenovationType,
                    nameof(ApartmentDetailsDto.RenovationType),
                    cancellationToken);

                if (!renovationTypeId.IsSuccess)
                    return renovationTypeId;

                apartmentDetails.RenovationTypeId = renovationTypeId.Data;
                apartmentDetails.RenovationType = null;
            }

            if (apartmentDetailsDto.BuildingMaterial != null)
            {
                var buildingMaterialId = await ResolveLookupIdAsync<BuildingMaterial>(
                    nameof(BuildingMaterial),
                    apartmentDetailsDto.BuildingMaterial,
                    nameof(ApartmentDetailsDto.BuildingMaterial),
                    cancellationToken);

                if (!buildingMaterialId.IsSuccess)
                    return buildingMaterialId;

                apartmentDetails.BuildingMaterialId = buildingMaterialId.Data;
                apartmentDetails.BuildingMaterial = null;
            }
        }
        else if (listingDto.ListingDetails is HouseDetailsDto houseDetailsDto
            && listing.ListingDetails is HouseDetails houseDetails)
        {
            if (houseDetailsDto.WallMaterial != null)
            {
                var wallMaterialId = await ResolveLookupIdAsync<WallMaterial>(
                    nameof(WallMaterial),
                    houseDetailsDto.WallMaterial,
                    nameof(HouseDetailsDto.WallMaterial),
                    cancellationToken);

                if (!wallMaterialId.IsSuccess)
                    return wallMaterialId;

                houseDetails.WallMaterialId = wallMaterialId.Data;
                houseDetails.WallMaterial = null;
            }

            if (houseDetailsDto.HeatingType != null)
            {
                var heatingTypeId = await ResolveLookupIdAsync<HeatingType>(
                    nameof(HeatingType),
                    houseDetailsDto.HeatingType,
                    nameof(HouseDetailsDto.HeatingType),
                    cancellationToken);

                if (!heatingTypeId.IsSuccess)
                    return heatingTypeId;

                houseDetails.HeatingTypeId = heatingTypeId.Data;
                houseDetails.HeatingType = null;
            }

            if (houseDetailsDto.SewerType != null)
            {
                var sewerTypeId = await ResolveLookupIdAsync<SewerType>(
                    nameof(SewerType),
                    houseDetailsDto.SewerType,
                    nameof(HouseDetailsDto.SewerType),
                    cancellationToken);

                if (!sewerTypeId.IsSuccess)
                    return sewerTypeId;

                houseDetails.SewerTypeId = sewerTypeId.Data;
                houseDetails.SewerType = null;
            }

            if (houseDetailsDto.GasType != null)
            {
                var gasTypeId = await ResolveLookupIdAsync<GasType>(
                    nameof(GasType),
                    houseDetailsDto.GasType,
                    nameof(HouseDetailsDto.GasType),
                    cancellationToken);

                if (!gasTypeId.IsSuccess)
                    return gasTypeId;

                houseDetails.GasTypeId = gasTypeId.Data;
                houseDetails.GasType = null;
            }

            if (houseDetailsDto.RoofMaterial != null)
            {
                var roofMaterialId = await ResolveLookupIdAsync<WallMaterial>(
                    nameof(WallMaterial),
                    houseDetailsDto.RoofMaterial,
                    nameof(HouseDetailsDto.RoofMaterial),
                    cancellationToken);

                if (!roofMaterialId.IsSuccess)
                    return roofMaterialId;

                houseDetails.RoofMaterialId = roofMaterialId.Data;
                houseDetails.RoofMaterial = null;
            }

            if (houseDetailsDto.WaterSupplyType != null)
            {
                var waterSupplyTypeId = await ResolveLookupIdAsync<WaterSupplyType>(
                    nameof(WaterSupplyType),
                    houseDetailsDto.WaterSupplyType,
                    nameof(HouseDetailsDto.WaterSupplyType),
                    cancellationToken);

                if (!waterSupplyTypeId.IsSuccess)
                    return waterSupplyTypeId;

                houseDetails.WaterSupplyTypeId = waterSupplyTypeId.Data;
                houseDetails.WaterSupplyType = null;
            }
        }
        else if (listingDto.ListingDetails is LandDetailsDto landDetailsDto
            && listing.ListingDetails is LandDetails landDetails)
        {
            if (landDetailsDto.LandCategory != null)
            {
                var landCategoryId = await ResolveLookupIdAsync<LandCategory>(
                    nameof(LandCategory),
                    landDetailsDto.LandCategory,
                    nameof(LandDetailsDto.LandCategory),
                    cancellationToken);

                if (!landCategoryId.IsSuccess)
                    return landCategoryId;

                landDetails.LandCategoryId = landCategoryId.Data;
                landDetails.LandCategory = null;
            }

            if (landDetailsDto.RoadAccessType != null)
            {
                var roadAccessTypeId = await ResolveLookupIdAsync<RoadAccessType>(
                    nameof(RoadAccessType),
                    landDetailsDto.RoadAccessType,
                    nameof(LandDetailsDto.RoadAccessType),
                    cancellationToken);

                if (!roadAccessTypeId.IsSuccess)
                    return roadAccessTypeId;

                landDetails.RoadAccessTypeId = roadAccessTypeId.Data;
                landDetails.RoadAccessType = null;
            }
        }
        else if (listingDto.ListingDetails is WarehouseDetailsDto warehouseDetailsDto
            && listing.ListingDetails is WarehouseDetails warehouseDetails)
        {
            if (warehouseDetailsDto.ClassType != null)
            {
                var classTypeId = await ResolveLookupIdAsync<ClassType>(
                    nameof(ClassType),
                    warehouseDetailsDto.ClassType,
                    nameof(WarehouseDetailsDto.ClassType),
                    cancellationToken);

                if (!classTypeId.IsSuccess)
                    return classTypeId;

                warehouseDetails.ClassTypeId = classTypeId.Data;
                warehouseDetails.ClassType = null;
            }

            if (warehouseDetailsDto.HeatingType != null)
            {
                var heatingTypeId = await ResolveLookupIdAsync<HeatingType>(
                    nameof(HeatingType),
                    warehouseDetailsDto.HeatingType,
                    nameof(WarehouseDetailsDto.HeatingType),
                    cancellationToken);

                if (!heatingTypeId.IsSuccess)
                    return heatingTypeId;

                warehouseDetails.HeatingTypeId = heatingTypeId.Data;
                warehouseDetails.HeatingType = null;
            }
        }
        else if (listingDto.ListingDetails is OfficeDetailsDto officeDetailsDto
            && listing.ListingDetails is OfficeDetails officeDetails)
        {
            if (officeDetailsDto.OfficeClass != null)
            {
                var officeClassId = await ResolveLookupIdAsync<OfficeClass>(
                    nameof(OfficeClass),
                    officeDetailsDto.OfficeClass,
                    nameof(OfficeDetailsDto.OfficeClass),
                    cancellationToken);

                if (!officeClassId.IsSuccess)
                    return officeClassId;

                officeDetails.OfficeClassId = officeClassId.Data;
                officeDetails.OfficeClass = null;
            }

            if (officeDetailsDto.LayoutType != null)
            {
                var layoutTypeId = await ResolveLookupIdAsync<LayoutType>(
                    nameof(LayoutType),
                    officeDetailsDto.LayoutType,
                    nameof(OfficeDetailsDto.LayoutType),
                    cancellationToken);

                if (!layoutTypeId.IsSuccess)
                    return layoutTypeId;

                officeDetails.LayoutTypeId = layoutTypeId.Data;
                officeDetails.LayoutType = null;
            }

            if (officeDetailsDto.RenovationType != null)
            {
                var renovationTypeId = await ResolveLookupIdAsync<LayoutType>(
                    nameof(LayoutType),
                    officeDetailsDto.RenovationType,
                    nameof(OfficeDetailsDto.RenovationType),
                    cancellationToken);

                if (!renovationTypeId.IsSuccess)
                    return renovationTypeId;

                officeDetails.RenovationTypeId = renovationTypeId.Data;
                officeDetails.RenovationType = null;
            }

            if (officeDetailsDto.ParkingType != null)
            {
                var parkingTypeId = await ResolveLookupIdAsync<LayoutType>(
                    nameof(LayoutType),
                    officeDetailsDto.ParkingType,
                    nameof(OfficeDetailsDto.ParkingType),
                    cancellationToken);

                if (!parkingTypeId.IsSuccess)
                    return parkingTypeId;

                officeDetails.ParkingTypeId = parkingTypeId.Data;
                officeDetails.ParkingType = null;
            }
        }

        return Result.Ok(Guid.Empty);
    }

    private async Task<Result<Guid>> ResolveLookupIdAsync<TLookupEntity>(
        string lookupName,
        string value,
        string propertyName,
        CancellationToken cancellationToken)
        where TLookupEntity : class, ILookupEntity
    {
        var id = await cache.ResolveIdAsync<TLookupEntity>(lookupName, value, cancellationToken);

        if (id != Guid.Empty)
            return Result.Ok(id);

        return Result.Validation<Guid>(
            $"{lookupName} was not found.",
            [new ResultError($"{lookupName} '{value}' was not found.", 0, propertyName)]);
    }
}
