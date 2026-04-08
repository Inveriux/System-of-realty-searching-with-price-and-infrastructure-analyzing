import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getConversationDetails } from "@diploma/api/chat";

export function useConversationDetails(api: ApiClient, conversationId: string | null | undefined, enabled = true) {
  return useQuery({
    queryKey: ["chat", "conversation", conversationId],
    queryFn: () => getConversationDetails(api, conversationId ?? ""),
    enabled: Boolean(conversationId) && enabled,
  });
}
