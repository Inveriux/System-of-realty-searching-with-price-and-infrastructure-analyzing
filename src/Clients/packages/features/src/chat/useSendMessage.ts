import { useMutation } from "@tanstack/react-query";
import type { ApiClient } from "@diploma/api/http";
import { sendMessage } from "@diploma/api/chat";
import type { SendMessageDto } from "@diploma/contracts/chat";

export function useSendMessage(api: ApiClient, conversationId: string | null | undefined) {
  return useMutation({
    mutationFn: (payload: SendMessageDto) => sendMessage(api, conversationId ?? "", payload),
  });
}
