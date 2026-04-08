import { z } from "zod";
import type {
  ConversationDetailsDto,
  ConversationListItemDto,
  ConversationMessagesPageDto,
  MessageDto,
  MessagesReadDto,
  SendMessageDto,
  StartConversationDto,
  UnreadCountDto,
} from "@diploma/contracts/chat";

export const StartConversationDtoSchema: z.ZodType<StartConversationDto> = z.object({
  listingId: z.string().uuid(),
  otherUserId: z.string().uuid().nullable().optional(),
});

export const SendMessageDtoSchema: z.ZodType<SendMessageDto> = z.object({
  text: z.string().min(1).max(4000),
  clientMessageId: z.string().max(100).nullable().optional(),
});

export const ConversationListItemDtoSchema: z.ZodType<ConversationListItemDto> = z.object({
  conversationId: z.string().uuid(),
  listingId: z.string().uuid(),
  listingTitle: z.string(),
  listingPrice: z.number().nullable(),
  listingPriceCurrency: z.string().nullable(),
  listingImageUrl: z.string().nullable(),
  otherUserId: z.string().uuid(),
  otherUserName: z.string(),
  otherUserAvatarUrl: z.string().nullable(),
  lastMessagePreview: z.string().nullable(),
  lastMessageAt: z.string().nullable(),
  unreadCount: z.number(),
});

export const ConversationDetailsDtoSchema: z.ZodType<ConversationDetailsDto> = z.object({
  conversationId: z.string().uuid(),
  listingId: z.string().uuid(),
  listingTitle: z.string(),
  listingPrice: z.number().nullable(),
  listingPriceCurrency: z.string().nullable(),
  listingImageUrl: z.string().nullable(),
  otherUserId: z.string().uuid(),
  otherUserName: z.string(),
  otherUserAvatarUrl: z.string().nullable(),
});

export const MessageDtoSchema: z.ZodType<MessageDto> = z.object({
  id: z.string().uuid(),
  conversationId: z.string().uuid(),
  senderUserId: z.string().uuid(),
  text: z.string(),
  createdAt: z.string(),
  isRead: z.boolean(),
  isMine: z.boolean(),
});

export const ConversationMessagesPageDtoSchema: z.ZodType<ConversationMessagesPageDto> = z.object({
  messages: z.array(MessageDtoSchema),
  page: z.number(),
  pageSize: z.number(),
  hasMore: z.boolean(),
});

export const UnreadCountDtoSchema: z.ZodType<UnreadCountDto> = z.object({
  count: z.number(),
});

export const MessagesReadDtoSchema: z.ZodType<MessagesReadDto> = z.object({
  conversationId: z.string().uuid(),
  readerUserId: z.string().uuid(),
  messageIds: z.array(z.string().uuid()),
  readAt: z.string(),
});
