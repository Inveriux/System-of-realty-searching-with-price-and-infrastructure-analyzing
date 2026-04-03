import type { ApiClient } from "./http";
import { z } from "zod";
import { ListingDtoSchema } from "@diploma/validation";
import { createPagedResponseSchema } from "@diploma/validation";
import type { ListingDetailsDto } from "@diploma/contracts/listings";

const LookupOptionSchema = z.object({
  name: z.string(),
});

const LookupOptionsSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.array(LookupOptionSchema),
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});

const CreateListingResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.string().uuid(),
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});

const PagedListingsSchema = createPagedResponseSchema(ListingDtoSchema);
const ListingsResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: PagedListingsSchema,
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});

const UpdateListingResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: ListingDtoSchema,
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});

const ActionResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.unknown().optional(),
  errors: z.array(
    z.object({
      message: z.string().optional(),
      errorMessage: z.string().optional(),
      code: z.number().nullable().optional(),
      propertyName: z.string().nullable().optional(),
    }),
  ),
});
const ActionResultOrVoidSchema = z.union([ActionResultSchema, z.void()]);

export type UpdateListingRequest = {
  agentId?: string;
  title: string;
  addressText: string;
  locationId?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  description?: string | null;
  priceAmount?: number | null;
  priceCurrency?: string | null;
  priceAmountByn?: number | null;
  priceAmountUsd?: number | null;
  pricePeriod?: string | null;
  areaTotal: number;
  buildYear?: number | null;
  publishedDate?: string | null;
  addressId: string;
  buildingType?: string | null;
  listingType: string;
  status?: string | null;
  listingDetailsType: string;
  mediaUrls?: string[] | null;
  listingDetails: ListingDetailsDto;
};

export async function getListings(api: ApiClient) {
  const result = await api.get("/api/listings/", ListingsResultSchema);
  return result.data;
}

export async function getDetailedListings(api: ApiClient) {
  const result = await api.get("/api/listings/details", ListingsResultSchema);
  return result.data;
}

export type LookupOption = z.infer<typeof LookupOptionSchema>;

async function getLookupOptions(api: ApiClient, path: string) {
  const result = await api.get(path, LookupOptionsSchema, { authMode: "optional" });
  return result.data;
}

export async function getCreateListingLookups(api: ApiClient) {
  const [
    listingTypes,
    buildingTypes,
    bathroomTypes,
    renovationTypes,
    buildingMaterials,
    classTypes,
    gasTypes,
    heatingTypes,
    landCategories,
    layoutTypes,
    officeClasses,
    roadAccessTypes,
    sewerTypes,
    wallMaterials,
    waterSupplyTypes,
  ] = await Promise.all([
    getLookupOptions(api, "/api/listing_types/"),
    getLookupOptions(api, "/api/building_types/"),
    getLookupOptions(api, "/api/bathroom_types/"),
    getLookupOptions(api, "/api/renovation_types/"),
    getLookupOptions(api, "/api/building_materials/"),
    getLookupOptions(api, "/api/class_types/"),
    getLookupOptions(api, "/api/gas_types/"),
    getLookupOptions(api, "/api/heating_types/"),
    getLookupOptions(api, "/api/land_categories/"),
    getLookupOptions(api, "/api/layout_types/"),
    getLookupOptions(api, "/api/office_classes/"),
    getLookupOptions(api, "/api/road_access_types/"),
    getLookupOptions(api, "/api/sewer_types/"),
    getLookupOptions(api, "/api/wall_materials/"),
    getLookupOptions(api, "/api/water_supply_types/"),
  ]);

  return {
    listingTypes,
    buildingTypes,
    bathroomTypes,
    renovationTypes,
    buildingMaterials,
    classTypes,
    gasTypes,
    heatingTypes,
    landCategories,
    layoutTypes,
    officeClasses,
    roadAccessTypes,
    sewerTypes,
    wallMaterials,
    waterSupplyTypes,
  };
}

export async function createListingWithImages(
  api: ApiClient,
  listing: Record<string, unknown>,
  images: File[],
) {
  const formData = new FormData();
  formData.append("listing", JSON.stringify(listing));

  for (const image of images) {
    formData.append("images", image);
  }

  const result = await api.post("/api/listings/with-images", formData, CreateListingResultSchema);
  return result.data;
}

export async function uploadListingImage(api: ApiClient, listingId: string, image: File) {
  const formData = new FormData();
  formData.append("image", image);
  await api.postFormVoid(`/api/listings/${listingId}/images`, formData);
}

export async function deleteListing(api: ApiClient, listingId: string) {
  await api.deleteVoid(`/api/listings/${listingId}`);
}

export async function hideListing(api: ApiClient, listingId: string) {
  await api.patch(`/api/listings/${listingId}/hide`, {}, ActionResultOrVoidSchema);
}

export async function unhideListing(api: ApiClient, listingId: string) {
  await api.patch(`/api/listings/${listingId}/unhide`, {}, ActionResultOrVoidSchema);
}

export async function updateListing(
  api: ApiClient,
  listingId: string,
  request: UpdateListingRequest,
) {
  const result = await api.patch(`/api/listings/${listingId}`, request, UpdateListingResultSchema);
  return result.data;
}
