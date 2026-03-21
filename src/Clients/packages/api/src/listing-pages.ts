import type { ApiClient } from "./http";
import { z } from "zod";
import { ListingPageDtoSchema } from "@diploma/validation";

const ListingPageResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: ListingPageDtoSchema,
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});

export async function getListingPage(api: ApiClient, listingId: string) {
  const result = await api.get(`/api/listing-pages/${listingId}`, ListingPageResultSchema, { authMode: "optional" });
  return result.data;
}
