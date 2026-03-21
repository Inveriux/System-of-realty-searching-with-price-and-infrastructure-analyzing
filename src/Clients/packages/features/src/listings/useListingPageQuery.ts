import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getListingPage } from "@diploma/api/listing-pages";

export function useListingPageQuery(api: ApiClient, listingId: string) {
  return useQuery({
    queryKey: ["listing-page", listingId],
    queryFn: () => getListingPage(api, listingId),
    enabled: Boolean(listingId),
  });
}
