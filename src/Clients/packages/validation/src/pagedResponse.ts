import { z } from "zod";
import type { PagedResponseDto } from "@diploma/contracts/pagedResponse";

export function createPagedResponseSchema<T>(
  itemSchema: z.ZodType<T>
): z.ZodType<PagedResponseDto<T>> {
  return z.object({
    items: z.array(itemSchema),
    page: z.number(),
    pageSize: z.number(),
    totalCount: z.number(),
  });
}