import type { SearchListingDto } from "@diploma/contracts/search";
import type { ListingCardItem } from "./components/listing-card.types";

type MapSearchListingOptions = {
  canDelete: boolean;
  canHide?: boolean;
  deletingId: string | null;
  hidingId?: string | null;
  onDelete: (id: string) => Promise<void>;
  onHide?: (id: string) => Promise<void>;
  isFavorite?: boolean;
  onToggleFavorite?: (id: string) => void | Promise<void>;
};

function inferRentalPricePeriod(listing: SearchListingDto) {
  if (listing.pricePeriod) {
    return listing.pricePeriod;
  }

  if (listing.listingType !== "Rent") {
    return undefined;
  }

  const description = (listing.description ?? "").toLowerCase();
  const title = (listing.title ?? "").toLowerCase();
  const source = `${title} ${description}`;

  if (source.includes("за сутки") || source.includes("посуточ")) {
    return "Daily";
  }

  if (source.includes("за месяц") || source.includes("в аренду")) {
    return "Monthly";
  }

  return undefined;
}

export function mapSearchListingToCard(
  listing: SearchListingDto,
  options: MapSearchListingOptions,
): ListingCardItem {
  const listingType: "Аренда" | "Продажа" = listing.listingType === "Rent" ? "Аренда" : "Продажа";
  const base = {
    id: listing.listingId,
    agentId: listing.agentId,
    title: listing.title,
    addressText: listing.addressText ?? "Адрес уточняется",
    priceAmount: (listing.priceAmountByn ?? listing.priceAmount) ?? undefined,
    priceAmountUsd: listing.priceAmountUsd ?? undefined,
    priceCurrency: (listing.priceCurrency ?? "BYN") as "BYN",
    pricePeriod: inferRentalPricePeriod(listing),
    areaTotal: listing.areaTotal,
    mainMedia: listing.mainMediaUrl,
    mediaCount: listing.mediaCount ?? undefined,
    listingType,
    buildingType: listing.buildingType,
    publishedDate: listing.publishedDate,
    isFavorite: options.isFavorite,
    canDelete: options.canDelete,
    isDeleting: options.deletingId === listing.listingId,
    canHide: options.canHide,
    isHiding: options.hidingId === listing.listingId,
    onDelete: options.onDelete,
    onHide: options.onHide,
    onToggleFavorite: options.onToggleFavorite,
    href: `/listings/${listing.listingId}`,
    infrastructureScore: listing.infrastructureScore,
    transportCount: listing.transportCount,
    stopGroupsCountPrimary: listing.stopGroupsCountPrimary,
    stopPlatformsCountPrimary: listing.stopPlatformsCountPrimary,
    educationCount: listing.educationCount,
    healthcareCount: listing.healthcareCount,
    transportNearestMeters: listing.transportNearestMeters,
    nearestStoreWalkMinutes: listing.nearestStoreWalkMinutes,
    nearestPharmacyWalkMinutes: listing.nearestPharmacyWalkMinutes,
    nearestStopGroupWalkMinutes: listing.nearestStopGroupWalkMinutes,
    nearestStopGroupDriveMinutes: listing.nearestStopGroupDriveMinutes,
    nearestStopGroupDriveMinutesEstimated: listing.nearestStopGroupDriveMinutesEstimated,
    nearestStopGroupName: listing.nearestStopGroupName,
    nearestStopGroupPlatformCount: listing.nearestStopGroupPlatformCount,
    nearestMetroWalkMinutes: listing.nearestMetroWalkMinutes,
    metroFoundInPrimaryRadius: listing.metroFoundInPrimaryRadius,
    nearestParkingWalkMinutes: listing.nearestParkingWalkMinutes,
    nearestParkingDriveMinutes: listing.nearestParkingDriveMinutes,
    nearestParkingDriveMinutesEstimated: listing.nearestParkingDriveMinutesEstimated,
    nearestClinicDriveMinutesEstimated: listing.nearestClinicDriveMinutesEstimated,
    nearestHospitalDriveMinutesEstimated: listing.nearestHospitalDriveMinutesEstimated,
    nearestParkWalkMinutes: listing.nearestParkWalkMinutes,
    nearestFitnessWalkMinutes: listing.nearestFitnessWalkMinutes,
    nearestFitnessDriveMinutes: listing.nearestFitnessDriveMinutes,
    relevanceScore: listing.relevanceScore,
    highlights: listing.highlights,
    hasCoordinates: listing.latitude != null && listing.longitude != null,
    nearestMetroName: listing.nearestMetroName,
  };

  switch (listing.listingDetailsType) {
    case "House":
      return {
        ...base,
        details: { kind: "house" },
      };
    case "Land":
      return {
        ...base,
        details: { kind: "land" },
      };
    case "Office":
      return {
        ...base,
        details: { kind: "office" },
      };
    case "Warehouse":
      return {
        ...base,
        details: { kind: "warehouse" },
      };
    case "Apartment":
    default:
      return {
        ...base,
        details: { kind: "apartment" },
      };
  }
}

export function getSearchPropertyTypeLabel(listing: SearchListingDto) {
  switch (listing.listingDetailsType) {
    case "House":
      return "Дом";
    case "Land":
      return "Участок";
    case "Office":
      return "Офис";
    case "Warehouse":
      return "Склад";
    case "Apartment":
    default:
      return "Квартира";
  }
}
