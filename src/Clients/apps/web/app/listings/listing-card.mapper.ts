import type {
  ApartmentDetailsDto,
  HouseDetailsDto,
  LandDetailsDto,
  ListingDto,
  OfficeDetailsDto,
  WarehouseDetailsDto,
} from "@diploma/contracts/listings";
import type { ListingCardItem } from "./components/listing-card.types";

type MapListingOptions = {
  canDelete: boolean;
  deletingId: string | null;
  onDelete: (id: string) => Promise<void>;
};

function stripNulls<T extends Record<string, unknown>>(value: T) {
  const entries = Object.entries(value).filter(([, entry]) => entry != null);
  return Object.fromEntries(entries) as {
    [K in keyof T]?: Exclude<T[K], null>;
  };
}

export function mapApiListingToCard(
  listing: ListingDto,
  options: MapListingOptions,
): ListingCardItem {
  const listingType = listing.listingType === "Rent" ? "Аренда" : "Продажа";
  const href = `/listings/${listing.id}`;

  switch (listing.listingDetailsType) {
    case "House":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "house",
          ...stripNulls(listing.listingDetails as HouseDetailsDto),
        },
      };
    case "Land":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "land",
          ...stripNulls(listing.listingDetails as LandDetailsDto),
        },
      };
    case "Office":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "office",
          ...stripNulls(listing.listingDetails as OfficeDetailsDto),
        },
      };
    case "Warehouse":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "warehouse",
          ...stripNulls(listing.listingDetails as WarehouseDetailsDto),
        },
      };
    case "Apartment":
    default:
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "apartment",
          ...stripNulls(listing.listingDetails as ApartmentDetailsDto),
        },
      };
  }
}

export function getPropertyTypeLabel(listing: ListingDto) {
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
