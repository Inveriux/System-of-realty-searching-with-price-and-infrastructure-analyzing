import { z } from "zod";
import type { ListingDto } from "@diploma/contracts/listings";

export const ListingDtoSchema: z.ZodType<ListingDto> = z.object({
  id: z.string(),
  title: z.string(),
  description: z.string().nullable(), 
  priceAmount: z.number().nullable(),
  priceCurrency: z.string().nullable(),
  mainMedia: z.string().nullable(), //TODO URL in property name 
});

export const ListingDtoArraySchema = z.array(ListingDtoSchema);