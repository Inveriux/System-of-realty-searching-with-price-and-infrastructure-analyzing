import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getMyConversations } from "@diploma/api/chat";

export function useMyConversations(api: ApiClient, enabled = true) {
  return useQuery({
    queryKey: ["chat", "conversations"],
    queryFn: () => getMyConversations(api),
    enabled,
  });
}
