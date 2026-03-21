import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getDetailedListings } from "@diploma/api/listings";

export function useListingsQuery(api: ApiClient) {
  return useQuery({
    queryKey: ["listings"],
    queryFn: () => getDetailedListings(api),
  });
}
