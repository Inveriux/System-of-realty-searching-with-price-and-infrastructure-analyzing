using ListingService.Application.Contracts.Cache;
using ListingService.Application.DTOs.Listings;
using ListingService.Domain.Models;
using FluentResults;
using AutoMapper;
using MediatR;

namespace ListingService.Application.UseCases.Listings;

public record CreateListingCommand(CreateListingDto ListingDto) : IRequest<Result>;

public class CreateListingHandler(IRepositoryManager manager, 
    ILookupCache cache, 
    IMapper mapper)
    : IRequestHandler<CreateListingCommand, Result>
{
    public async Task<Result> Handle(CreateListingCommand request, 
        CancellationToken cancellationToken)
    {   
        var listing = mapper.Map<Listing>(request.ListingDto);
        await manager.Listing.AddAsync(listing, cancellationToken);

        await ResolveLookupIds(listing, request.ListingDto, cancellationToken);

        await manager.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task ResolveLookupIds(Listing listing, CreateListingDto listingDto, CancellationToken cancellationToken)
    {
        if (listingDto.BuildingType != null)
        {
            listing.BuildingTypeId = await cache.ResolveIdAsync<BuildingType>(
                nameof(BuildingType),
                listingDto.BuildingType,
                cancellationToken);
            listing.BuildingType = null;
        }

        listing.ListingTypeId = await cache.ResolveIdAsync<ListingType>(
            nameof(ListingType),
            listingDto.ListingType,
            cancellationToken);

        if(listingDto.ListingDetails is ApartmentDetailsDto apartmentDetailsDto
            && listing.ListingDetails is ApartmentDetails apartmentDetails)
        {
            if (apartmentDetailsDto.BathroomType != null)
            {
                apartmentDetails.BathroomTypeId = await cache.ResolveIdAsync<BathroomType>(
                    nameof(BathroomType),
                    apartmentDetailsDto.BathroomType,
                    cancellationToken);
                apartmentDetails.BathroomType = null;
            }

            if (apartmentDetailsDto.RenovationType != null)
            {
                apartmentDetails.RenovationTypeId = await cache.ResolveIdAsync<RenovationType>(
                    nameof(RenovationType),
                    apartmentDetailsDto.RenovationType,
                    cancellationToken);
                apartmentDetails.RenovationType = null;
            }

            if (apartmentDetailsDto.BuildingMaterial != null)
            {
                apartmentDetails.BuildingMaterialId = await cache.ResolveIdAsync<BuildingMaterial>(
                    nameof(BuildingMaterial),
                    apartmentDetailsDto.BuildingMaterial,
                    cancellationToken);
                apartmentDetails.BuildingMaterial = null;
            }
        }
        else if (listingDto.ListingDetails is HouseDetailsDto houseDetailsDto
            && listing.ListingDetails is HouseDetails houseDetails)
        {
            if (houseDetailsDto.WallMaterial != null)
            {
                houseDetails.WallMaterialId = await cache.ResolveIdAsync<WallMaterial>(
                    nameof(WallMaterial),
                    houseDetailsDto.WallMaterial,
                    cancellationToken);
                houseDetails.WallMaterial = null;
            }

            if (houseDetailsDto.HeatingType != null)
            {
                houseDetails.HeatingTypeId = await cache.ResolveIdAsync<HeatingType>(
                    nameof(HeatingType),
                    houseDetailsDto.HeatingType,
                    cancellationToken);
                houseDetails.HeatingType = null;
            }

            if (houseDetailsDto.SewerType != null)
            {
                houseDetails.SewerTypeId = await cache.ResolveIdAsync<SewerType>(
                    nameof(SewerType),
                    houseDetailsDto.SewerType,
                    cancellationToken);
                houseDetails.SewerType = null;
            }

            if (houseDetailsDto.GasType != null)
            {
                houseDetails.GasTypeId = await cache.ResolveIdAsync<GasType>(
                    nameof(GasType),
                    houseDetailsDto.GasType,
                    cancellationToken);
                houseDetails.GasType = null;
            }

            if (houseDetailsDto.RoofMaterial != null)
            {
                houseDetails.RoofMaterialId = await cache.ResolveIdAsync<WallMaterial>(
                    nameof(WallMaterial),
                    houseDetailsDto.RoofMaterial,
                    cancellationToken);
                houseDetails.RoofMaterial = null;
            }

            if (houseDetailsDto.WaterSupplyType != null)
            {
                houseDetails.WaterSupplyTypeId = await cache.ResolveIdAsync<WaterSupplyType>(
                    nameof(WaterSupplyType),
                    houseDetailsDto.WaterSupplyType,
                    cancellationToken);
                houseDetails.WaterSupplyType = null;
            }
        }
        else if (listingDto.ListingDetails is LandDetailsDto landDetailsDto
            && listing.ListingDetails is LandDetails landDetails)
        {
            if (landDetailsDto.LandCategory != null)
            {
                landDetails.LandCategoryId = await cache.ResolveIdAsync<LandCategory>(
                    nameof(LandCategory),
                    landDetailsDto.LandCategory,
                    cancellationToken);
                landDetails.LandCategory = null;
            }

            if (landDetailsDto.RoadAccessType != null)
            {
                landDetails.RoadAccessTypeId = await cache.ResolveIdAsync<RoadAccessType>(
                    nameof(RoadAccessType),
                    landDetailsDto.RoadAccessType,
                    cancellationToken);
                landDetails.RoadAccessType = null;
            }
        }
        else if (listingDto.ListingDetails is WarehouseDetailsDto warehouseDetailsDto
            && listing.ListingDetails is WarehouseDetails warehouseDetails)
        {
            if (warehouseDetailsDto.ClassType != null)
            {
                warehouseDetails.ClassTypeId = await cache.ResolveIdAsync<ClassType>(
                    nameof(ClassType),
                    warehouseDetailsDto.ClassType,
                    cancellationToken);
                warehouseDetails.ClassType = null;
            }

            if (warehouseDetailsDto.HeatingType != null)
            {
                warehouseDetails.HeatingTypeId = await cache.ResolveIdAsync<HeatingType>(
                    nameof(HeatingType),
                    warehouseDetailsDto.HeatingType,
                    cancellationToken);
                warehouseDetails.HeatingType = null;
            }
        }
        else if (listingDto.ListingDetails is OfficeDetailsDto officeDetailsDto
            && listing.ListingDetails is OfficeDetails officeDetails)
        {
            if (officeDetailsDto.OfficeClass != null)
            {
                officeDetails.OfficeClassId = await cache.ResolveIdAsync<OfficeClass>(
                    nameof(OfficeClass),
                    officeDetailsDto.OfficeClass,
                    cancellationToken);
                officeDetails.OfficeClass = null;
            }

            if (officeDetailsDto.LayoutType != null)
            {
                officeDetails.LayoutTypeId = await cache.ResolveIdAsync<LayoutType>(
                    nameof(LayoutType),
                    officeDetailsDto.LayoutType,
                    cancellationToken);
                officeDetails.LayoutType = null;
            }

            if (officeDetailsDto.RenovationType != null)
            {
                officeDetails.RenovationTypeId = await cache.ResolveIdAsync<LayoutType>(
                    nameof(LayoutType),
                    officeDetailsDto.RenovationType,
                    cancellationToken);
                officeDetails.RenovationType = null;
            }

            if (officeDetailsDto.ParkingType != null)
            {
                officeDetails.ParkingTypeId = await cache.ResolveIdAsync<LayoutType>(
                    nameof(LayoutType),
                    officeDetailsDto.ParkingType,
                    cancellationToken);
                officeDetails.ParkingType = null;
            }
        }
    }
}
