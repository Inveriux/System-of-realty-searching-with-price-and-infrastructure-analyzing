import { z } from "zod";
import type { ApiClient } from "./http";
import { SearchListingsRequestSchema, SearchListingsResponseSchema } from "@diploma/validation";
import type { SearchListingsRequest, SearchListingsResponse } from "@diploma/contracts/search";

export type { SearchListingsRequest, SearchListingsResponse } from "@diploma/contracts/search";

const SearchListingsResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: SearchListingsResponseSchema,
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});

function createEmptySearchResponse(request: SearchListingsRequest): SearchListingsResponse {
  return {
    items: [],
    sort: request.sortBy ?? request.sort ?? "relevance",
    searchProfile: request.searchProfile ?? "default",
    appliedWeights: null,
    page: request.page,
    pageSize: request.pageSize,
    totalCount: 0,
  };
}

function isServiceUnavailableError(error: unknown) {
  if (!(error instanceof Error)) {
    return false;
  }

  const message = error.message.toLowerCase();
  return (
    message.includes("сервис временно недоступен")
    || message.includes("не удалось выполнить запрос")
    || message.includes("failed to fetch")
    || message.includes("backendnotfound")
  );
}

export async function searchListings(api: ApiClient, request: SearchListingsRequest): Promise<SearchListingsResponse> {
  const payload = SearchListingsRequestSchema.parse(request);
  try {
    const result = await api.post("/api/search/listings", payload, SearchListingsResultSchema, {
      authMode: "optional",
    });

    return result.data;
  } catch (error) {
    if (isServiceUnavailableError(error)) {
      return createEmptySearchResponse(payload);
    }

    throw error;
  }
}
