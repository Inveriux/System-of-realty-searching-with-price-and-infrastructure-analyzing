import type { SearchListingDto } from "@diploma/contracts/search";
import type { ListingCardItem } from "./components/listing-card.types";

type MapSearchListingOptions = {
  canDelete: boolean;
  deletingId: string | null;
  onDelete: (id: string) => Promise<void>;
};

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
    areaTotal: listing.areaTotal,
    mainMedia: listing.mainMediaUrl,
    listingType,
    buildingType: listing.buildingType,
    publishedDate: listing.publishedDate,
    canDelete: options.canDelete,
    isDeleting: options.deletingId === listing.listingId,
    onDelete: options.onDelete,
    href: `/listings/${listing.listingId}`,
    infrastructureScore: listing.infrastructureScore,
    transportCount: listing.transportCount,
    educationCount: listing.educationCount,
    healthcareCount: listing.healthcareCount,
    transportNearestMeters: listing.transportNearestMeters,
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
