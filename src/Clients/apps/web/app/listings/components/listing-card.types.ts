export type DealType = "Продажа" | "Аренда";
export type PriceCurrency = "RUB" | "USD" | "EUR" | "BYN" | string;
export type PricePeriod = "Месяц" | "Сутки" | "Неделя" | "Год" | string | null;

export type ListingCardBase = {
  id: string;
  agentId?: string;
  title: string;
  addressText?: string;
  priceAmount?: number;
  priceAmountUsd?: number;
  priceCurrency?: PriceCurrency;
  pricePeriod?: PricePeriod;
  areaTotal: number;
  mainMedia?: string | null;
  listingType: DealType;
  buildingType?: string | null;
  publishedDate?: string | null;
  href?: string;
  isFavorite?: boolean;
  canDelete?: boolean;
  isDeleting?: boolean;
  mediaCount?: number;
  infrastructureScore?: number | null;
  transportCount?: number | null;
  educationCount?: number | null;
  healthcareCount?: number | null;
  transportNearestMeters?: number | null;
  onDelete?: (id: string) => void | Promise<void>;
};

export type ApartmentDetails = {
  kind: "apartment";
  rooms?: number;
  bedrooms?: number;
  floorsTotal?: number;
  levels?: number;
  floor?: number;
  areaLiving?: number;
  areaKitchen?: number;
  hasBalcony?: boolean;
  hasLoggia?: boolean;
  hasElevator?: boolean;
  ceilingHeight?: number;
  bathroomType?: string;
  renovationType?: string;
  buildingMaterial?: string;
};

export type HouseDetails = {
  kind: "house";
  rooms?: number;
  bedrooms?: number;
  floors?: number;
  houseArea?: number;
  plotArea?: number;
  hasElectricity?: boolean;
  hasGarage?: boolean;
  hasBasement?: boolean;
  hasFence?: boolean;
  wallMaterial?: string;
  heatingType?: string;
  sewerType?: string;
  gasType?: string;
  roofMaterial?: string;
  waterSupplyType?: string;
};

export type LandDetails = {
  kind: "land";
  plotArea?: number;
  allowedUse?: string;
  hasElectricity?: boolean;
  hasGas?: boolean;
  hasWater?: boolean;
  hasSewer?: boolean;
  hasBuildings?: boolean;
  landCategory?: string;
  roadAccessType?: string;
};

export type OfficeDetails = {
  kind: "office";
  plotArea?: number;
  floor?: number;
  floorsTotal?: number;
  levels?: number;
  parkingSpaces?: number;
  kitchenette?: boolean;
  officeClass?: string;
  layoutType?: string;
  renovationType?: string;
  parkingType?: string;
};

export type WarehouseDetails = {
  kind: "warehouse";
  plotArea?: number;
  ceilingHeight?: number;
  floorLoadTonsPerSquareMeter?: number;
  gateCount?: number;
  hasDockLevelers?: boolean;
  dockCount?: number;
  hasRamps?: boolean;
  powerKw?: number;
  hasOffice?: boolean;
  officeArea?: number;
  classType?: string;
  heatingType?: string;
};

export type ListingCardItem =
  | (ListingCardBase & { details: ApartmentDetails })
  | (ListingCardBase & { details: HouseDetails })
  | (ListingCardBase & { details: LandDetails })
  | (ListingCardBase & { details: OfficeDetails })
  | (ListingCardBase & { details: WarehouseDetails });

export type ListingPrimarySpec = {
  label: string;
  value: string;
};

export type ListingSecondaryFeature = {
  label: string;
  tone?: "neutral" | "accent";
};
