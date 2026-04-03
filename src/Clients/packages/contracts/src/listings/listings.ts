export type ListingMediaDto = {
  url: string;
};

export type ListingPriceHistoryDto = {
  changedAt: string;
  priceAmount: number | null;
  priceCurrency: string | null;
  priceAmountByn: number | null;
  priceAmountUsd: number | null;
};

export type ApartmentDetailsDto = {
  rooms: number | null;
  bedrooms: number | null;
  floorsTotal: number | null;
  levels: number | null;
  floor: number | null;
  areaLiving: number | null;
  areaKitchen: number | null;
  hasBalcony: boolean | null;
  hasLoggia: boolean | null;
  hasElevator: boolean | null;
  ceilingHeight: number | null;
  bathroomType: string | null;
  renovationType: string | null;
  buildingMaterial: string | null;
};

export type HouseDetailsDto = {
  rooms: number | null;
  bedrooms: number | null;
  floors: number | null;
  houseArea: number | null;
  plotArea: number | null;
  hasElectricity: boolean | null;
  hasGarage: boolean | null;
  hasBasement: boolean | null;
  hasFence: boolean | null;
  wallMaterial: string | null;
  heatingType: string | null;
  sewerType: string | null;
  gasType: string | null;
  roofMaterial: string | null;
  waterSupplyType: string | null;
};

export type LandDetailsDto = {
  plotArea: number | null;
  allowedUse: string | null;
  hasElectricity: boolean | null;
  hasGas: boolean | null;
  hasWater: boolean | null;
  hasSewer: boolean | null;
  hasBuildings: boolean | null;
  landCategory: string | null;
  roadAccessType: string | null;
};

export type OfficeDetailsDto = {
  plotArea: number | null;
  floor: number | null;
  floorsTotal: number | null;
  levels: number | null;
  parkingSpaces: number | null;
  kitchenette: boolean | null;
  officeClass: string | null;
  layoutType: string | null;
  renovationType: string | null;
  parkingType: string | null;
};

export type WarehouseDetailsDto = {
  plotArea: number | null;
  ceilingHeight: number | null;
  floorLoadTonsPerSquareMeter: number | null;
  gateCount: number | null;
  hasDockLevelers: boolean | null;
  dockCount: number | null;
  hasRamps: boolean | null;
  powerKw: number | null;
  hasOffice: boolean | null;
  officeArea: number | null;
  classType: string | null;
  heatingType: string | null;
};

export type ListingDetailsDto =
  | ApartmentDetailsDto
  | HouseDetailsDto
  | LandDetailsDto
  | OfficeDetailsDto
  | WarehouseDetailsDto;

export type ListingDto = {
  id: string;
  agentId: string;
  addressText: string | null;
  locationId: string | null;
  latitude: number | null;
  longitude: number | null;
  title: string;
  description: string | null;
  priceAmount: number | null;
  priceCurrency: string | null;
  priceAmountByn: number | null;
  priceAmountUsd: number | null;
  pricePeriod: string | null;
  areaTotal: number;
  buildYear: number | null;
  publishedDate: string | null;
  status?: string | null;
  buildingType: string | null;
  listingType: string;
  mainMedia: string | null;
  media: ListingMediaDto[] | null;
  priceHistory: ListingPriceHistoryDto[] | null;
  listingDetailsType: string | null;
  listingDetails: ListingDetailsDto;
  created: string | null;
  createdBy: string | null;
  lastModified: string | null;
  lastModifiedBy: string | null;
};
