import type { ApiClient } from "./http";
import { z } from "zod";
import { ListingDtoSchema } from "@diploma/validation";
import { createPagedResponseSchema } from "@diploma/validation";

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

export async function deleteListing(api: ApiClient, listingId: string) {
  await api.deleteVoid(`/api/listings/${listingId}`);
}
