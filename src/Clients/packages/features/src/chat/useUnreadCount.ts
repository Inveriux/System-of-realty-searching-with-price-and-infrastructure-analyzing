import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getUnreadCount } from "@diploma/api/chat";

export function useUnreadCount(api: ApiClient, enabled = true) {
  return useQuery({
    queryKey: ["chat", "unread-count"],
    queryFn: () => getUnreadCount(api),
    enabled,
    refetchInterval: enabled ? 60000 : false,
  });
}
