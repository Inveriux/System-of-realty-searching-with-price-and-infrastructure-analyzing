import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { searchListings } from "@diploma/api/search";
import type { SearchListingsRequest } from "@diploma/api/search";

export function useSearchListingsQuery(api: ApiClient, request: SearchListingsRequest) {
  return useQuery({
    queryKey: ["search-listings", request],
    queryFn: () => searchListings(api, request),
  });
}
