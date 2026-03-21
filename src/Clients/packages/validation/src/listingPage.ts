import { z } from "zod";
import type { ListingPageDto } from "@diploma/contracts/listingPage";

const nullableString = z.string().nullable();
const nullableNumber = z.number().nullable();
const nullableBoolean = z.boolean().nullable();

const ListingPageMediaDtoSchema = z.object({
  url: z.string(),
});

const ListingPagePriceHistoryDtoSchema = z.object({
  changedAt: z.string(),
  priceAmount: nullableNumber,
  priceCurrency: nullableString,
  priceAmountByn: nullableNumber,
  priceAmountUsd: nullableNumber,
});

const ListingPageListingDtoSchema = z.object({
  id: z.string(),
  agentId: z.string(),
  locationId: nullableString,
  title: z.string(),
  description: nullableString,
  addressText: nullableString,
  latitude: nullableNumber,
  longitude: nullableNumber,
  priceAmount: nullableNumber,
  priceCurrency: nullableString,
  priceAmountByn: nullableNumber,
  priceAmountUsd: nullableNumber,
  pricePeriod: nullableString,
  areaTotal: z.number(),
  buildYear: nullableNumber,
  publishedDate: nullableString,
  buildingType: nullableString,
  listingType: z.string(),
  mainMedia: nullableString,
  media: z.array(ListingPageMediaDtoSchema),
  listingDetailsType: nullableString,
  listingDetails: z.record(z.string(), z.unknown()).nullable(),
  priceHistory: z.array(ListingPagePriceHistoryDtoSchema),
});

const ListingPagePoiDtoSchema = z.object({
  id: z.string(),
  externalId: nullableString,
  name: z.string(),
  addressText: nullableString,
  latitude: z.number(),
  longitude: z.number(),
  category: z.string(),
  distanceMeters: z.number(),
});

const ListingPageGroupedPoisDtoSchema = z.object({
  category: z.string(),
  count: z.number(),
  pois: z.array(ListingPagePoiDtoSchema),
});

const ListingPageInfrastructureDtoSchema = z.object({
  score: nullableNumber,
  totalScore: nullableNumber,
  educationScore: nullableNumber,
  healthcareScore: nullableNumber,
  transportScore: nullableNumber,
  commerceScore: nullableNumber,
  leisureScore: nullableNumber,
  poiCount: z.number(),
  summary: nullableString,
  groupedPois: z.array(ListingPageGroupedPoisDtoSchema),
  nearbyPois: z.array(ListingPagePoiDtoSchema),
  highlights: z.array(z.string()),
  calculatedAtUtc: nullableString,
});

const ListingPageSellerDtoSchema = z.object({
  id: z.string(),
  fullName: nullableString,
  avatarUrl: nullableString,
  phone: nullableString,
  email: nullableString,
  rating: nullableNumber,
  reviewsCount: nullableNumber,
  isVerified: nullableBoolean,
  agencyName: nullableString,
  isFallback: z.boolean(),
});

const ListingQuickStatDtoSchema = z.object({
  key: z.string(),
  label: z.string(),
  value: z.string(),
  unit: nullableString,
});

const ListingFeatureTagDtoSchema = z.object({
  key: z.string(),
  label: z.string(),
  tone: nullableString,
});

const ListingPageWarningDtoSchema = z.object({
  code: z.string(),
  message: z.string(),
  severity: z.string(),
  source: nullableString,
});

const ListingPageDerivedDtoSchema = z.object({
  quickStats: z.array(ListingQuickStatDtoSchema),
  featureTags: z.array(ListingFeatureTagDtoSchema),
  warnings: z.array(z.string()),
  lastUpdatedUtc: z.string(),
});

const ListingPageAvailableBlocksDtoSchema = z.object({
  listing: z.boolean(),
  infrastructure: z.boolean(),
  seller: z.boolean(),
  priceHistory: z.boolean(),
  map: z.boolean(),
});

export const ListingPageDtoSchema: z.ZodType<ListingPageDto> = z.object({
  listing: ListingPageListingDtoSchema,
  infrastructure: ListingPageInfrastructureDtoSchema.nullable(),
  seller: ListingPageSellerDtoSchema,
  derived: ListingPageDerivedDtoSchema,
  availableBlocks: ListingPageAvailableBlocksDtoSchema,
  warnings: z.array(ListingPageWarningDtoSchema),
});
