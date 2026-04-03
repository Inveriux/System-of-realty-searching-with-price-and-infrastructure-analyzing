import { z } from "zod";
import type { ListingDto } from "@diploma/contracts/listings";

const nullableString = z.string().nullable();
const nullableNumber = z.number().nullable();
const nullableBoolean = z.boolean().nullable();

const ListingMediaDtoSchema = z.object({
  url: z.string(),
});

const ListingPriceHistoryDtoSchema = z.object({
  changedAt: z.string(),
  priceAmount: nullableNumber,
  priceCurrency: nullableString,
  priceAmountByn: nullableNumber,
  priceAmountUsd: nullableNumber,
});

const ApartmentDetailsDtoSchema = z.object({
  rooms: nullableNumber,
  bedrooms: nullableNumber,
  floorsTotal: nullableNumber,
  levels: nullableNumber,
  floor: nullableNumber,
  areaLiving: nullableNumber,
  areaKitchen: nullableNumber,
  hasBalcony: nullableBoolean,
  hasLoggia: nullableBoolean,
  hasElevator: nullableBoolean,
  ceilingHeight: nullableNumber,
  bathroomType: nullableString,
  renovationType: nullableString,
  buildingMaterial: nullableString,
});

const HouseDetailsDtoSchema = z.object({
  rooms: nullableNumber,
  bedrooms: nullableNumber,
  floors: nullableNumber,
  houseArea: nullableNumber,
  plotArea: nullableNumber,
  hasElectricity: nullableBoolean,
  hasGarage: nullableBoolean,
  hasBasement: nullableBoolean,
  hasFence: nullableBoolean,
  wallMaterial: nullableString,
  heatingType: nullableString,
  sewerType: nullableString,
  gasType: nullableString,
  roofMaterial: nullableString,
  waterSupplyType: nullableString,
});

const LandDetailsDtoSchema = z.object({
  plotArea: nullableNumber,
  allowedUse: nullableString,
  hasElectricity: nullableBoolean,
  hasGas: nullableBoolean,
  hasWater: nullableBoolean,
  hasSewer: nullableBoolean,
  hasBuildings: nullableBoolean,
  landCategory: nullableString,
  roadAccessType: nullableString,
});

const OfficeDetailsDtoSchema = z.object({
  plotArea: nullableNumber,
  floor: nullableNumber,
  floorsTotal: nullableNumber,
  levels: nullableNumber,
  parkingSpaces: nullableNumber,
  kitchenette: nullableBoolean,
  officeClass: nullableString,
  layoutType: nullableString,
  renovationType: nullableString,
  parkingType: nullableString,
});

const WarehouseDetailsDtoSchema = z.object({
  plotArea: nullableNumber,
  ceilingHeight: nullableNumber,
  floorLoadTonsPerSquareMeter: nullableNumber,
  gateCount: nullableNumber,
  hasDockLevelers: nullableBoolean,
  dockCount: nullableNumber,
  hasRamps: nullableBoolean,
  powerKw: nullableNumber,
  hasOffice: nullableBoolean,
  officeArea: nullableNumber,
  classType: nullableString,
  heatingType: nullableString,
});

const ListingDetailsDtoSchema = z.union([
  ApartmentDetailsDtoSchema,
  HouseDetailsDtoSchema,
  LandDetailsDtoSchema,
  OfficeDetailsDtoSchema,
  WarehouseDetailsDtoSchema,
]);

export const ListingDtoSchema: z.ZodType<ListingDto> = z.object({
  id: z.string(),
  agentId: z.string(),
  addressText: nullableString,
  locationId: nullableString,
  latitude: nullableNumber,
  longitude: nullableNumber,
  title: z.string(),
  description: nullableString,
  priceAmount: nullableNumber,
  priceCurrency: nullableString,
  priceAmountByn: nullableNumber,
  priceAmountUsd: nullableNumber,
  pricePeriod: nullableString,
  areaTotal: z.number(),
  buildYear: nullableNumber,
  publishedDate: nullableString,
  status: z.string().nullish().default("Published"),
  buildingType: nullableString,
  listingType: z.string(),
  mainMedia: nullableString,
  media: z.array(ListingMediaDtoSchema).nullable(),
  priceHistory: z.array(ListingPriceHistoryDtoSchema).nullable(),
  listingDetailsType: nullableString,
  listingDetails: ListingDetailsDtoSchema,
  created: nullableString,
  createdBy: nullableString,
  lastModified: nullableString,
  lastModifiedBy: nullableString,
});

export const ListingDtoArraySchema = z.array(ListingDtoSchema);
