export type ConversationType = "listing" | "support";

export type StartConversationDto = {
  listingId: string;
  otherUserId?: string | null;
};

export type SendMessageDto = {
  text: string;
  clientMessageId?: string | null;
};

export type ConversationListItemDto = {
  conversationId: string;
  conversationType: ConversationType;
  listingId: string;
  listingTitle: string;
  listingPrice: number | null;
  listingPriceCurrency: string | null;
  listingImageUrl: string | null;
  otherUserId: string;
  otherUserName: string;
  otherUserAvatarUrl: string | null;
  lastMessagePreview: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
};

export type ConversationDetailsDto = {
  conversationId: string;
  conversationType: ConversationType;
  listingId: string;
  listingTitle: string;
  listingPrice: number | null;
  listingPriceCurrency: string | null;
  listingImageUrl: string | null;
  otherUserId: string;
  otherUserName: string;
  otherUserAvatarUrl: string | null;
  otherUserEmail: string | null;
  otherUserPhone: string | null;
};

export type MessageDto = {
  id: string;
  conversationId: string;
  senderUserId: string;
  text: string;
  createdAt: string;
  isRead: boolean;
  isMine: boolean;
};

export type ConversationMessagesPageDto = {
  messages: MessageDto[];
  page: number;
  pageSize: number;
  hasMore: boolean;
};

export type UnreadCountDto = {
  count: number;
};

export type MessagesReadDto = {
  conversationId: string;
  readerUserId: string;
  messageIds: string[];
  readAt: string;
};
