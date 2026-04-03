using AutoMapper;
using BasicBuildingBlocks.Services.Abstractions;
using ListingService.Application.Contracts.Cache;
using ListingService.Application.Contracts.Services;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Abstractions;
using ListingService.Domain.Enums;
using ListingService.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ListingService.Application.UseCases.Listings;

public sealed record UpdateListingCommand(Guid Id, UpdateListingRequestDto Request)
    : IRequest<Result<GetDetailedListingDto>>;

public sealed class UpdateListingHandler(
    IRepositoryManager repositoryManager,
    ILookupCache cache,
    IMapper mapper,
    IInfrastructureServiceClient infrastructureServiceClient,
    ICurrencyRateService currencyRateService,
    ISearchServiceClient searchServiceClient,
    ILogger<UpdateListingHandler> logger)
    : IRequestHandler<UpdateListingCommand, Result<GetDetailedListingDto>>
{
    public async Task<Result<GetDetailedListingDto>> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetDetailedByIdAsync(request.Id, true, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<GetDetailedListingDto>(
                "Объявление не найдено.",
                [new ResultError("Объявление не найдено.", 0, nameof(request.Id))]);
        }

        var oldLocationId = listing.LocationId;
        var oldDetailsType = listing.ListingDetailsType;

        listing.AgentId = request.Request.AgentId == Guid.Empty ? listing.AgentId : request.Request.AgentId;
        listing.Title = request.Request.Title;
        listing.AddressText = request.Request.AddressText;
        listing.Description = request.Request.Description ?? string.Empty;
        listing.AreaTotal = request.Request.AreaTotal;
        listing.BuildYear = request.Request.BuildYear;
        listing.AddressId = request.Request.AddressId;
        listing.PricePeriod = request.Request.PricePeriod;
        listing.Status = request.Request.Status ?? listing.Status;
        listing.PublishedDate = request.Request.PublishedDate?.ToUniversalTime();
        listing.ListingDetailsType = request.Request.ListingDetailsType;

        await NormalizePriceAsync(listing, request.Request, cancellationToken);
        await ResolveLocationAsync(listing, request.Request, cancellationToken);

        var detailsChanged = !string.Equals(oldDetailsType, request.Request.ListingDetailsType, StringComparison.Ordinal);
        if (detailsChanged)
        {
            var replacementDetails = mapper.Map<IListingDetails>(request.Request.ListingDetails);
            replacementDetails.ListingId = listing.Id;
            listing.ListingDetails = replacementDetails;
        }

        ApplyMediaUrls(listing, request.Request.MediaUrls);

        var resolveLookupIdsResult = await ResolveLookupIds(listing, request.Request, cancellationToken);
        if (!resolveLookupIdsResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to resolve lookup ids while updating listing {ListingId}. Message: {Message}. Errors: {Errors}",
                listing.Id,
                resolveLookupIdsResult.Message,
                string.Join("; ", resolveLookupIdsResult.Errors.Select(static e => $"{e.PropertyName}: {e.ErrorMessage}")));

            return Result.Fail<GetDetailedListingDto>(
                resolveLookupIdsResult.StatusCode,
                resolveLookupIdsResult.Message,
                resolveLookupIdsResult.Errors);
        }

        if (detailsChanged && listing.ListingDetails is not null)
        {
            await repositoryManager.Listing.ReplaceDetailsAsync(
                listing.Id,
                listing.ListingDetailsType,
                listing.ListingDetails,
                cancellationToken);
        }

        await repositoryManager.SaveChangesAsync(cancellationToken);

        var postCommitCancellationToken = CancellationToken.None;
        if (listing.LocationId.HasValue &&
            (!oldLocationId.HasValue || oldLocationId.Value != listing.LocationId.Value))
        {
            try
            {
                var refreshed = await infrastructureServiceClient.RefreshInfrastructureAsync(
                    listing.LocationId.Value,
                    postCommitCancellationToken);

                if (!refreshed)
                {
                    logger.LogWarning(
                        "Listing {ListingId} was updated, but infrastructure refresh failed for location {LocationId}.",
                        listing.Id,
                        listing.LocationId.Value);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Listing {ListingId} was updated, but infrastructure refresh threw for location {LocationId}.",
                    listing.Id,
                    listing.LocationId.Value);
            }
        }

        await searchServiceClient.IndexListingAsync(new SearchListingIndexRequest
        {
            ListingId = listing.Id,
            AgentId = listing.AgentId,
            Title = listing.Title,
            Description = listing.Description,
            AddressText = listing.AddressText,
            LocationId = listing.LocationId,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            ListingType = listing.ListingType?.Name,
            BuildingType = listing.BuildingType?.Name,
            PriceAmount = listing.PriceAmount,
            PriceAmountByn = listing.PriceAmountByn,
            PriceAmountUsd = listing.PriceAmountUsd,
            PriceCurrency = listing.PriceCurrency,
            AreaTotal = listing.AreaTotal,
            MainMediaUrl = listing.Media?.FirstOrDefault(x => x.IsMainMedia)?.Url,
            MediaCount = listing.Media?.Count ?? 0,
            PublishedDate = listing.PublishedDate,
            ListingDetailsType = listing.ListingDetailsType
        }, cancellationToken);

        return Result.Ok(mapper.Map<GetDetailedListingDto>(listing));
    }

    private static void ApplyMediaUrls(Listing listing, List<string>? mediaUrls)
    {
        listing.Media ??= [];
        listing.Media.Clear();

        if (mediaUrls is null)
            return;

        var normalizedUrls = mediaUrls
            .Where(static url => !string.IsNullOrWhiteSpace(url))
            .Distinct()
            .ToList();

        for (var index = 0; index < normalizedUrls.Count; index++)
        {
            listing.Media.Add(new ListingMedia
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                Url = normalizedUrls[index],
                IsMainMedia = index == 0
            });
        }
    }

    private async Task ResolveLocationAsync(
        Listing listing,
        UpdateListingRequestDto request,
        CancellationToken cancellationToken)
    {
        listing.LocationId = request.LocationId;
        listing.Latitude = request.Latitude;
        listing.Longitude = request.Longitude;

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
    }

    private async Task NormalizePriceAsync(Listing listing, UpdateListingRequestDto request, CancellationToken cancellationToken)
    {
        var bynAmount = request.PriceAmountByn ?? request.PriceAmount;
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
    }

    private async Task<Result<Guid>> ResolveLookupIds(
        Listing listing,
        UpdateListingRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.BuildingType is not null)
        {
            var buildingTypeId = await ResolveLookupIdAsync<BuildingType>(
                nameof(BuildingType),
                request.BuildingType,
                nameof(UpdateListingRequestDto.BuildingType),
                cancellationToken);

            if (!buildingTypeId.IsSuccess)
                return buildingTypeId;

            listing.BuildingTypeId = buildingTypeId.Data;
            listing.BuildingType = null;
        }

        var listingTypeId = await ResolveLookupIdAsync<ListingType>(
            nameof(ListingType),
            request.ListingType,
            nameof(UpdateListingRequestDto.ListingType),
            cancellationToken);

        if (!listingTypeId.IsSuccess)
            return listingTypeId;

        listing.ListingTypeId = listingTypeId.Data;
        listing.ListingType = null!;

        if (request.ListingDetails is ApartmentDetailsDto apartmentDetailsDto
            && listing.ListingDetails is ApartmentDetails apartmentDetails)
        {
            apartmentDetails.Rooms = apartmentDetailsDto.Rooms;
            apartmentDetails.Bedrooms = apartmentDetailsDto.Bedrooms;
            apartmentDetails.FloorsTotal = apartmentDetailsDto.FloorsTotal;
            apartmentDetails.Levels = apartmentDetailsDto.Levels;
            apartmentDetails.Floor = apartmentDetailsDto.Floor;
            apartmentDetails.AreaLiving = apartmentDetailsDto.AreaLiving;
            apartmentDetails.AreaKitchen = apartmentDetailsDto.AreaKitchen;
            apartmentDetails.HasBalcony = apartmentDetailsDto.HasBalcony;
            apartmentDetails.HasLoggia = apartmentDetailsDto.HasLoggia;
            apartmentDetails.HasElevator = apartmentDetailsDto.HasElevator;
            apartmentDetails.CeilingHeight = apartmentDetailsDto.CeilingHeight;

            if (apartmentDetailsDto.BathroomType is not null)
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

            if (apartmentDetailsDto.RenovationType is not null)
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

            if (apartmentDetailsDto.BuildingMaterial is not null)
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
        else if (request.ListingDetails is HouseDetailsDto houseDetailsDto
            && listing.ListingDetails is HouseDetails houseDetails)
        {
            houseDetails.Rooms = houseDetailsDto.Rooms;
            houseDetails.Bedrooms = houseDetailsDto.Bedrooms;
            houseDetails.Floors = houseDetailsDto.Floors;
            houseDetails.HouseArea = houseDetailsDto.HouseArea;
            houseDetails.PlotArea = houseDetailsDto.PlotArea;
            houseDetails.HasElectricity = houseDetailsDto.HasElectricity;
            houseDetails.HasGarage = houseDetailsDto.HasGarage;
            houseDetails.HasBasement = houseDetailsDto.HasBasement;
            houseDetails.HasFence = houseDetailsDto.HasFence;

            if (houseDetailsDto.WallMaterial is not null)
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

            if (houseDetailsDto.HeatingType is not null)
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

            if (houseDetailsDto.SewerType is not null)
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

            if (houseDetailsDto.GasType is not null)
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

            if (houseDetailsDto.RoofMaterial is not null)
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

            if (houseDetailsDto.WaterSupplyType is not null)
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
        else if (request.ListingDetails is LandDetailsDto landDetailsDto
            && listing.ListingDetails is LandDetails landDetails)
        {
            landDetails.PlotArea = landDetailsDto.PlotArea;
            landDetails.AllowedUse = landDetailsDto.AllowedUse;
            landDetails.HasElectricity = landDetailsDto.HasElectricity;
            landDetails.HasGas = landDetailsDto.HasGas;
            landDetails.HasWater = landDetailsDto.HasWater;
            landDetails.HasSewer = landDetailsDto.HasSewer;
            landDetails.HasBuildings = landDetailsDto.HasBuildings;

            if (landDetailsDto.LandCategory is not null)
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

            if (landDetailsDto.RoadAccessType is not null)
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
        else if (request.ListingDetails is WarehouseDetailsDto warehouseDetailsDto
            && listing.ListingDetails is WarehouseDetails warehouseDetails)
        {
            warehouseDetails.PlotArea = warehouseDetailsDto.PlotArea;
            warehouseDetails.CeilingHeight = warehouseDetailsDto.CeilingHeight;
            warehouseDetails.FloorLoadTonsPerSquareMeter = warehouseDetailsDto.FloorLoadTonsPerSquareMeter;
            warehouseDetails.GateCount = warehouseDetailsDto.GateCount;
            warehouseDetails.HasDockLevelers = warehouseDetailsDto.HasDockLevelers;
            warehouseDetails.DockCount = warehouseDetailsDto.DockCount;
            warehouseDetails.HasRamps = warehouseDetailsDto.HasRamps;
            warehouseDetails.PowerKw = warehouseDetailsDto.PowerKw;
            warehouseDetails.HasOffice = warehouseDetailsDto.HasOffice;
            warehouseDetails.OfficeArea = warehouseDetailsDto.OfficeArea;

            if (warehouseDetailsDto.ClassType is not null)
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

            if (warehouseDetailsDto.HeatingType is not null)
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
        else if (request.ListingDetails is OfficeDetailsDto officeDetailsDto
            && listing.ListingDetails is OfficeDetails officeDetails)
        {
            officeDetails.PlotArea = officeDetailsDto.PlotArea;
            officeDetails.Floor = officeDetailsDto.Floor;
            officeDetails.FloorsTotal = officeDetailsDto.FloorsTotal;
            officeDetails.Levels = officeDetailsDto.Levels;
            officeDetails.ParkingSpaces = officeDetailsDto.ParkingSpaces;
            officeDetails.Kitchenette = officeDetailsDto.Kitchenette;

            if (officeDetailsDto.OfficeClass is not null)
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

            if (officeDetailsDto.LayoutType is not null)
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

            if (officeDetailsDto.RenovationType is not null)
            {
                var renovationTypeId = await ResolveLookupIdAsync<RenovationType>(
                    nameof(RenovationType),
                    officeDetailsDto.RenovationType,
                    nameof(OfficeDetailsDto.RenovationType),
                    cancellationToken);

                if (!renovationTypeId.IsSuccess)
                    return renovationTypeId;

                officeDetails.RenovationTypeId = renovationTypeId.Data;
                officeDetails.RenovationType = null;
            }

            if (officeDetailsDto.ParkingType is not null)
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
            $"Значение справочника {lookupName} не найдено.",
            [new ResultError($"Значение '{value}' для справочника {lookupName} не найдено.", 0, propertyName)]);
    }
}
