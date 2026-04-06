import { z } from "zod";
import { ApiClient } from "./http";

const ResolvedLocationSchema = z.object({
  id: z.string().uuid(),
  addressText: z.string(),
  normalizedAddressText: z.string(),
  latitude: z.number(),
  longitude: z.number(),
  country: z.string().nullable().optional(),
  region: z.string().nullable().optional(),
  city: z.string().nullable().optional(),
  street: z.string().nullable().optional(),
  houseNumber: z.string().nullable().optional(),
});

export type ResolveLocationRequest = {
  addressText?: string | null;
  latitude?: number | null;
  longitude?: number | null;
};

export type ResolvedLocation = z.infer<typeof ResolvedLocationSchema>;

export async function resolveLocation(api: ApiClient, request: ResolveLocationRequest) {
  return api.post("/api/locations/resolve", request, ResolvedLocationSchema);
}
