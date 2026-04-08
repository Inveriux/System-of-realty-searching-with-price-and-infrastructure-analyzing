import { useMutation } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { startConversation } from "@diploma/api/chat";
import type { StartConversationDto } from "@diploma/contracts/chat";

export function useStartConversation(api: ApiClient) {
  return useMutation({
    mutationFn: (payload: StartConversationDto) => startConversation(api, payload),
  });
}
