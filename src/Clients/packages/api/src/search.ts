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

export async function searchListings(api: ApiClient, request: SearchListingsRequest): Promise<SearchListingsResponse> {
  const payload = SearchListingsRequestSchema.parse(request);
  const result = await api.post("/api/search/listings", payload, SearchListingsResultSchema, {
    authMode: "optional",
  });

  return result.data;
}
