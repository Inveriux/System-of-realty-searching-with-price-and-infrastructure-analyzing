import { z } from "zod";
import type { ApiClient } from "./http";
import {
  CreateListingPriceHistoryEntryRequestSchema,
  ListingPriceComparisonSchema,
  ListingPriceHistorySchema,
  PricingAnalyticsCategorySchema,
  PricingAnalyticsPointSchema,
  PricingAnalyticsSummarySchema,
  PricingEstimateRequestSchema,
  PricingEstimateResponseSchema,
  UpdateListingPriceHistoryEntryRequestSchema,
  UpdateListingPriceRequestSchema,
} from "@diploma/validation";
import type {
  CreateListingPriceHistoryEntryRequest,
  ListingPriceComparison,
  ListingPriceHistory,
  PricingAnalyticsCategory,
  PricingAnalyticsPoint,
  PricingAnalyticsQuery,
  PricingAnalyticsSummary,
  PricingEstimateRequest,
  PricingEstimateResponse,
  UpdateListingPriceHistoryEntryRequest,
  UpdateListingPriceRequest,
} from "@diploma/contracts/pricing-analytics";

const PricingAnalyticsSummaryResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: PricingAnalyticsSummarySchema,
  errors: z.array(z.object({
    message: z.string().optional(),
    errorMessage: z.string().optional(),
    code: z.number().nullable().optional(),
    propertyName: z.string().nullable().optional(),
  })),
});

const PricingAnalyticsPointsResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.array(PricingAnalyticsPointSchema),
  errors: z.array(z.object({
    message: z.string().optional(),
    errorMessage: z.string().optional(),
    code: z.number().nullable().optional(),
    propertyName: z.string().nullable().optional(),
  })),
});

const PricingAnalyticsCategoriesResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.array(PricingAnalyticsCategorySchema),
  errors: z.array(z.object({
    message: z.string().optional(),
    errorMessage: z.string().optional(),
    code: z.number().nullable().optional(),
    propertyName: z.string().nullable().optional(),
  })),
});

const ListingPriceComparisonResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: ListingPriceComparisonSchema,
  errors: z.array(z.object({
    message: z.string().optional(),
    errorMessage: z.string().optional(),
    code: z.number().nullable().optional(),
    propertyName: z.string().nullable().optional(),
  })),
});

const PricingEstimateResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: PricingEstimateResponseSchema,
  errors: z.array(z.object({
    message: z.string().optional(),
    errorMessage: z.string().optional(),
    code: z.number().nullable().optional(),
    propertyName: z.string().nullable().optional(),
  })),
});

const ListingPriceHistoryResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: ListingPriceHistorySchema,
  errors: z.array(z.object({
    message: z.string().optional(),
    errorMessage: z.string().optional(),
    code: z.number().nullable().optional(),
    propertyName: z.string().nullable().optional(),
  })),
});

function buildQueryString(query: PricingAnalyticsQuery) {
  const params = new URLSearchParams();

  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") {
      return;
    }

    params.set(key, String(value));
  });

  const serialized = params.toString();
  return serialized ? `?${serialized}` : "";
}

export async function getPricingAnalyticsSummary(api: ApiClient, query: PricingAnalyticsQuery): Promise<PricingAnalyticsSummary> {
  const result = await api.get(`/api/pricing-analytics/summary${buildQueryString(query)}`, PricingAnalyticsSummaryResultSchema, { authMode: "optional" });
  return result.data;
}

export async function getPricingAnalyticsTimeSeries(api: ApiClient, query: PricingAnalyticsQuery): Promise<PricingAnalyticsPoint[]> {
  const result = await api.get(`/api/pricing-analytics/timeseries${buildQueryString(query)}`, PricingAnalyticsPointsResultSchema, { authMode: "optional" });
  return result.data;
}

export async function getPricingAnalyticsCategories(api: ApiClient, query: PricingAnalyticsQuery): Promise<PricingAnalyticsCategory[]> {
  const result = await api.get(`/api/pricing-analytics/categories${buildQueryString(query)}`, PricingAnalyticsCategoriesResultSchema, { authMode: "optional" });
  return result.data;
}

export async function getListingPriceComparison(api: ApiClient, listingId: string): Promise<ListingPriceComparison> {
  const result = await api.get(`/api/pricing-analytics/listings/${listingId}/comparison`, ListingPriceComparisonResultSchema, { authMode: "optional" });
  return result.data;
}

export async function estimateListingPrice(api: ApiClient, request: PricingEstimateRequest): Promise<PricingEstimateResponse> {
  const payload = PricingEstimateRequestSchema.parse(request);
  const result = await api.post("/api/pricing-analytics/estimate", payload, PricingEstimateResultSchema, { authMode: "optional" });
  return result.data;
}

export async function getListingPriceHistory(api: ApiClient, listingId: string): Promise<ListingPriceHistory> {
  const result = await api.get(`/api/pricing-analytics/listings/${listingId}/price-history`, ListingPriceHistoryResultSchema, { authMode: "optional" });
  return result.data;
}

export async function updateListingPrice(api: ApiClient, listingId: string, request: UpdateListingPriceRequest): Promise<ListingPriceHistory> {
  const payload = UpdateListingPriceRequestSchema.parse(request);
  const result = await api.patch(`/api/pricing-analytics/listings/${listingId}/price`, payload, ListingPriceHistoryResultSchema);
  return result.data;
}

export async function addListingPriceHistoryEntry(api: ApiClient, listingId: string, request: CreateListingPriceHistoryEntryRequest): Promise<ListingPriceHistory> {
  const payload = CreateListingPriceHistoryEntryRequestSchema.parse(request);
  const result = await api.post(`/api/pricing-analytics/listings/${listingId}/price-history`, payload, ListingPriceHistoryResultSchema);
  return result.data;
}

export async function updateListingPriceHistoryEntry(
  api: ApiClient,
  listingId: string,
  historyEntryId: string,
  request: UpdateListingPriceHistoryEntryRequest,
): Promise<ListingPriceHistory> {
  const payload = UpdateListingPriceHistoryEntryRequestSchema.parse(request);
  const result = await api.patch(`/api/pricing-analytics/listings/${listingId}/price-history/${historyEntryId}`, payload, ListingPriceHistoryResultSchema);
  return result.data;
}

export async function deleteListingPriceHistoryEntry(api: ApiClient, listingId: string, historyEntryId: string): Promise<ListingPriceHistory> {
  const result = await api.delete(`/api/pricing-analytics/listings/${listingId}/price-history/${historyEntryId}`, ListingPriceHistoryResultSchema);
  return result.data;
}
