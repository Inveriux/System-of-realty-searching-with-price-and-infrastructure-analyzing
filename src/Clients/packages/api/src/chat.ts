import { z } from "zod";
import type { ApiClient } from "./http";
import {
  ConversationDetailsDtoSchema,
  ConversationListItemDtoSchema,
  ConversationMessagesPageDtoSchema,
  MessageDtoSchema,
  UnreadCountDtoSchema,
} from "@diploma/validation";
import type { SendMessageDto, StartConversationDto } from "@diploma/contracts/chat";

const ConversationDetailsResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: ConversationDetailsDtoSchema,
  errors: z.array(z.unknown()),
});

const ConversationListResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.array(ConversationListItemDtoSchema),
  errors: z.array(z.unknown()),
});

const ConversationMessagesResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: ConversationMessagesPageDtoSchema,
  errors: z.array(z.unknown()),
});

const MessageResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: MessageDtoSchema,
  errors: z.array(z.unknown()),
});

const UnreadCountResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: UnreadCountDtoSchema,
  errors: z.array(z.unknown()),
});

export async function startConversation(api: ApiClient, payload: StartConversationDto) {
  const result = await api.post("/api/chat/conversations/start", payload, ConversationDetailsResultSchema);
  return result.data;
}

export async function startSupportConversation(api: ApiClient) {
  const result = await api.post("/api/chat/conversations/support/start", {}, ConversationDetailsResultSchema);
  return result.data;
}

export async function getMyConversations(api: ApiClient) {
  const result = await api.get("/api/chat/conversations", ConversationListResultSchema);
  return result.data;
}

export async function getConversationDetails(api: ApiClient, conversationId: string) {
  const result = await api.get(`/api/chat/conversations/${conversationId}`, ConversationDetailsResultSchema);
  return result.data;
}

export async function getConversationMessages(api: ApiClient, conversationId: string, page = 1, pageSize = 30) {
  const result = await api.get(
    `/api/chat/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`,
    ConversationMessagesResultSchema,
  );
  return result.data;
}

export async function sendMessage(api: ApiClient, conversationId: string, payload: SendMessageDto) {
  const result = await api.post(`/api/chat/conversations/${conversationId}/messages`, payload, MessageResultSchema);
  return result.data;
}

export async function markConversationAsRead(api: ApiClient, conversationId: string) {
  await api.postVoid(`/api/chat/conversations/${conversationId}/read`, {});
}

export async function getUnreadCount(api: ApiClient) {
  const result = await api.get("/api/chat/unread-count", UnreadCountResultSchema);
  return result.data;
}
