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
  canHide?: boolean;
  canUnhide?: boolean;
  canEdit?: boolean;
  canEditPrice?: boolean;
  deletingId: string | null;
  hidingId?: string | null;
  unhidingId?: string | null;
  compactLikeCatalog?: boolean;
  onDelete: (id: string) => Promise<void>;
  onHide?: (id: string) => Promise<void>;
  onUnhide?: (id: string) => Promise<void>;
  onEdit?: (id: string) => void | Promise<void>;
  onEditPrice?: (id: string) => void | Promise<void>;
  isFavorite?: boolean;
  onToggleFavorite?: (id: string) => void | Promise<void>;
};

function stripNulls<T extends Record<string, unknown>>(value: T) {
  const entries = Object.entries(value).filter(([, entry]) => entry != null);
  return Object.fromEntries(entries) as {
    [K in keyof T]?: Exclude<T[K], null>;
  };
}

function normalizeOfficeDetails(details: OfficeDetailsDto | Record<string, unknown>) {
  const source = details as Record<string, unknown>;

  return stripNulls({
    plotArea: (source.plotArea ?? source.PlotArea) as number | null | undefined,
    floor: (source.floor ?? source.Floor) as number | null | undefined,
    floorsTotal: (source.floorsTotal ?? source.FloorsTotal) as number | null | undefined,
    levels: (source.levels ?? source.Levels) as number | null | undefined,
    parkingSpaces: (source.parkingSpaces ?? source.ParkingSpaces) as number | null | undefined,
    kitchenette: (source.kitchenette ?? source.Kitchenette) as boolean | null | undefined,
    officeClass: (source.officeClass ?? source.OfficeClass) as string | null | undefined,
    layoutType: (source.layoutType ?? source.LayoutType) as string | null | undefined,
    renovationType: (source.renovationType ?? source.RenovationType ?? source.officeRenovationType) as string | null | undefined,
    parkingType: (source.parkingType ?? source.ParkingType) as string | null | undefined,
  });
}

function mapCompactDetails(listing: ListingDto): ListingCardItem["details"] {
  switch (listing.listingDetailsType) {
    case "House": {
      const details = listing.listingDetails as HouseDetailsDto;
      return stripNulls({
        kind: "house" as const,
        plotArea: details?.plotArea ?? undefined,
        floors: details?.floors ?? undefined,
      });
    }
    case "Land":
      return { kind: "land" };
    case "Office":
      return { kind: "office" };
    case "Warehouse":
      return { kind: "warehouse" };
    case "Apartment":
    default:
      return { kind: "apartment" };
  }
}

export function mapApiListingToCard(
  listing: ListingDto,
  options: MapListingOptions,
): ListingCardItem {
  const listingType = listing.listingType === "Rent" ? "Аренда" : "Продажа";
  const href = `/listings/${listing.id}`;
  const compactDetails = options.compactLikeCatalog ? mapCompactDetails(listing) : null;

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
        isHidden: listing.status === "Hidden",
        isFavorite: options.isFavorite,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        canHide: options.canHide,
        isHiding: options.hidingId === listing.id,
        canUnhide: options.canUnhide,
        isUnhiding: options.unhidingId === listing.id,
        canEdit: options.canEdit,
        canEditPrice: options.canEditPrice,
        onDelete: options.onDelete,
        onHide: options.onHide,
        onUnhide: options.onUnhide,
        onEdit: options.onEdit,
        onEditPrice: options.onEditPrice,
        onToggleFavorite: options.onToggleFavorite,
        href,
        details: compactDetails ?? {
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
        isHidden: listing.status === "Hidden",
        isFavorite: options.isFavorite,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        canHide: options.canHide,
        isHiding: options.hidingId === listing.id,
        canUnhide: options.canUnhide,
        isUnhiding: options.unhidingId === listing.id,
        canEdit: options.canEdit,
        canEditPrice: options.canEditPrice,
        onDelete: options.onDelete,
        onHide: options.onHide,
        onUnhide: options.onUnhide,
        onEdit: options.onEdit,
        onEditPrice: options.onEditPrice,
        onToggleFavorite: options.onToggleFavorite,
        href,
        details: compactDetails ?? {
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
        isHidden: listing.status === "Hidden",
        isFavorite: options.isFavorite,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        canHide: options.canHide,
        isHiding: options.hidingId === listing.id,
        canUnhide: options.canUnhide,
        isUnhiding: options.unhidingId === listing.id,
        canEdit: options.canEdit,
        canEditPrice: options.canEditPrice,
        onDelete: options.onDelete,
        onHide: options.onHide,
        onUnhide: options.onUnhide,
        onEdit: options.onEdit,
        onEditPrice: options.onEditPrice,
        onToggleFavorite: options.onToggleFavorite,
        href,
        details: compactDetails ?? {
          kind: "office",
          ...normalizeOfficeDetails(listing.listingDetails as OfficeDetailsDto),
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
        isHidden: listing.status === "Hidden",
        isFavorite: options.isFavorite,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        canHide: options.canHide,
        isHiding: options.hidingId === listing.id,
        canUnhide: options.canUnhide,
        isUnhiding: options.unhidingId === listing.id,
        canEdit: options.canEdit,
        canEditPrice: options.canEditPrice,
        onDelete: options.onDelete,
        onHide: options.onHide,
        onUnhide: options.onUnhide,
        onEdit: options.onEdit,
        onEditPrice: options.onEditPrice,
        onToggleFavorite: options.onToggleFavorite,
        href,
        details: compactDetails ?? {
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
        isHidden: listing.status === "Hidden",
        isFavorite: options.isFavorite,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        canHide: options.canHide,
        isHiding: options.hidingId === listing.id,
        canUnhide: options.canUnhide,
        isUnhiding: options.unhidingId === listing.id,
        canEdit: options.canEdit,
        canEditPrice: options.canEditPrice,
        onDelete: options.onDelete,
        onHide: options.onHide,
        onUnhide: options.onUnhide,
        onEdit: options.onEdit,
        onEditPrice: options.onEditPrice,
        onToggleFavorite: options.onToggleFavorite,
        href,
        details: compactDetails ?? {
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
