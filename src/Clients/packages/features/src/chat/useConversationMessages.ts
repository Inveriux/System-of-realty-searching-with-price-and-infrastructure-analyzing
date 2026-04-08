import { useQuery } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { getConversationMessages } from "@diploma/api/chat";

export function useConversationMessages(
  api: ApiClient,
  conversationId: string | null | undefined,
  page = 1,
  pageSize = 30,
  enabled = true,
) {
  return useQuery({
    queryKey: ["chat", "messages", conversationId, page, pageSize],
    queryFn: () => getConversationMessages(api, conversationId ?? "", page, pageSize),
    enabled: Boolean(conversationId) && enabled,
  });
}
