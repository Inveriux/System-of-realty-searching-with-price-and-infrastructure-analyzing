import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getCurrentUser } from "@diploma/api/users";

export function useCurrentUserQuery(api: ApiClient, enabled = true) {
  return useQuery({
    queryKey: ["current-user"],
    queryFn: () => getCurrentUser(api),
    enabled,
    retry: false,
  });
}
