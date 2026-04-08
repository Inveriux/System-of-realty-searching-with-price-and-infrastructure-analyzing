using ChatService.DTOs.Chat;

namespace ChatService.Services.Abstractions;

public interface IChatRealtimeNotifier
{
    Task NotifyMessageReceivedAsync(Guid conversationId, MessageDto message, CancellationToken cancellationToken);
    Task NotifyConversationUpdatedAsync(Guid userId, ConversationListItemDto item, CancellationToken cancellationToken);
    Task NotifyMessagesReadAsync(Guid conversationId, Guid userId, MessagesReadDto dto, CancellationToken cancellationToken);
}
