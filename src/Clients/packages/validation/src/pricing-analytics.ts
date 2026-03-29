import { z } from "zod";
import type {
  CreateListingPriceHistoryEntryRequest,
  ListingPriceComparison,
  ListingPriceHistory,
  PricingAnalyticsCategory,
  PricingAnalyticsConfidenceLevel,
  PricingAnalyticsGeography,
  PricingAnalyticsListingType,
  PricingAnalyticsPoint,
  PricingAnalyticsSummary,
  PricingAnalyticsTrendDirection,
  PricingEstimateRequest,
  PricingEstimateResponse,
  UpdateListingPriceHistoryEntryRequest,
  UpdateListingPriceRequest,
} from "@diploma/contracts/pricing-analytics";

const nullableString = z.string().nullable();
const nullableNumber = z.number().nullable();

export const PricingAnalyticsListingTypeSchema: z.ZodType<PricingAnalyticsListingType> = z.enum(["sale", "rent"]);
export const PricingAnalyticsGeographySchema: z.ZodType<PricingAnalyticsGeography> = z.enum(["country", "minsk", "city"]);
export const PricingAnalyticsTrendDirectionSchema: z.ZodType<PricingAnalyticsTrendDirection> = z.enum(["up", "down", "flat"]);
export const PricingAnalyticsConfidenceLevelSchema: z.ZodType<PricingAnalyticsConfidenceLevel> = z.enum(["low", "medium", "high"]);
export const ListingRelativePositionSchema = z.enum(["below_average", "near_average", "above_average", "no_data"]);

export const PricingAnalyticsSummarySchema: z.ZodType<PricingAnalyticsSummary> = z.object({
  averagePricePerSqm: nullableNumber,
  medianPricePerSqm: nullableNumber,
  minPricePerSqm: nullableNumber,
  maxPricePerSqm: nullableNumber,
  marketRangeLowPricePerSqm: nullableNumber,
  marketRangeHighPricePerSqm: nullableNumber,
  listingsCount: z.number(),
  periodStart: nullableString,
  periodEnd: nullableString,
  updatedAt: z.string(),
  confidenceLevel: PricingAnalyticsConfidenceLevelSchema,
  hasEnoughData: z.boolean(),
  trendDirection: PricingAnalyticsTrendDirectionSchema,
});

export const PricingAnalyticsPointSchema: z.ZodType<PricingAnalyticsPoint> = z.object({
  period: z.string(),
  averagePricePerSqm: nullableNumber,
  medianPricePerSqm: nullableNumber,
  listingsCount: z.number(),
  hasEnoughData: z.boolean(),
  confidenceLevel: PricingAnalyticsConfidenceLevelSchema,
});

export const PricingAnalyticsCategorySchema: z.ZodType<PricingAnalyticsCategory> = z.object({
  categoryKey: z.string(),
  categoryLabel: z.string(),
  averagePricePerSqm: nullableNumber,
  medianPricePerSqm: nullableNumber,
  minPricePerSqm: nullableNumber,
  maxPricePerSqm: nullableNumber,
  marketRangeLowPricePerSqm: nullableNumber,
  marketRangeHighPricePerSqm: nullableNumber,
  listingsCount: z.number(),
  trendDirection: PricingAnalyticsTrendDirectionSchema,
  confidenceLevel: PricingAnalyticsConfidenceLevelSchema,
  hasEnoughData: z.boolean(),
});

export const ListingPriceComparisonSchema: z.ZodType<ListingPriceComparison> = z.object({
  currentListingPricePerSqm: nullableNumber,
  localRadiusAveragePricePerSqm: nullableNumber,
  localRadiusMedianPricePerSqm: nullableNumber,
  localRadiusListingsCount: z.number(),
  cityAveragePricePerSqm: nullableNumber,
  cityMedianPricePerSqm: nullableNumber,
  cityListingsCount: z.number(),
  countryAveragePricePerSqm: nullableNumber,
  countryMedianPricePerSqm: nullableNumber,
  countryListingsCount: z.number(),
  marketRangeLowPricePerSqm: nullableNumber,
  marketRangeHighPricePerSqm: nullableNumber,
  relativePosition: ListingRelativePositionSchema,
  message: z.string(),
  confidenceLevel: PricingAnalyticsConfidenceLevelSchema,
  hasEnoughData: z.boolean(),
});

export const PricingEstimateRequestSchema: z.ZodType<PricingEstimateRequest> = z.object({
  listingType: nullableString.optional(),
  buildingType: nullableString.optional(),
  listingDetailsType: nullableString.optional(),
  priceAmountByn: z.number(),
  areaTotal: z.number(),
  latitude: nullableNumber.optional(),
  longitude: nullableNumber.optional(),
  cityName: nullableString.optional(),
  locationId: nullableString.optional(),
});

export const PricingEstimateResponseSchema: z.ZodType<PricingEstimateResponse> = z.object({
  enteredPricePerSqm: nullableNumber,
  localRadiusAveragePricePerSqm: nullableNumber,
  cityAveragePricePerSqm: nullableNumber,
  countryAveragePricePerSqm: nullableNumber,
  localRadiusListingsCount: z.number(),
  cityListingsCount: z.number(),
  countryListingsCount: z.number(),
  relativePosition: ListingRelativePositionSchema,
  message: z.string(),
  confidenceLevel: PricingAnalyticsConfidenceLevelSchema,
  hasEnoughData: z.boolean(),
});

export const ListingPriceHistoryPointSchema = z.object({
  id: z.string(),
  effectiveDateUtc: z.string(),
  priceAmountByn: nullableNumber,
  pricePerSqmByn: nullableNumber,
  comment: nullableString,
});

export const ListingPriceHistorySchema: z.ZodType<ListingPriceHistory> = z.object({
  listingId: z.string(),
  currentPriceAmountByn: nullableNumber,
  firstRecordedPriceAmountByn: nullableNumber,
  lastRecordedPriceAmountByn: nullableNumber,
  firstRecordedAt: nullableString,
  lastRecordedAt: nullableString,
  percentChangeFromFirst: nullableNumber,
  history: z.array(ListingPriceHistoryPointSchema),
});

export const UpdateListingPriceRequestSchema: z.ZodType<UpdateListingPriceRequest> = z.object({
  priceAmountByn: z.number(),
  effectiveDateUtc: nullableString.optional(),
  comment: nullableString.optional(),
});

export const CreateListingPriceHistoryEntryRequestSchema: z.ZodType<CreateListingPriceHistoryEntryRequest> = z.object({
  priceAmountByn: z.number(),
  effectiveDateUtc: z.string(),
  comment: nullableString.optional(),
});

export const UpdateListingPriceHistoryEntryRequestSchema: z.ZodType<UpdateListingPriceHistoryEntryRequest> = z.object({
  priceAmountByn: nullableNumber.optional(),
  effectiveDateUtc: nullableString.optional(),
  comment: nullableString.optional(),
});
