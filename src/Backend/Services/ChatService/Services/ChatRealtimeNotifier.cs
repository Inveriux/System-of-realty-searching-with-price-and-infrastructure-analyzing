using ChatService.DTOs.Chat;
using ChatService.Hubs;
using ChatService.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Services;

public sealed class ChatRealtimeNotifier(
    IHubContext<ChatHub> hubContext,
    ILogger<ChatRealtimeNotifier> logger) : IChatRealtimeNotifier
{
    public async Task NotifyMessageReceivedAsync(Guid conversationId, MessageDto message, CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.Group(ChatHub.BuildConversationGroupName(conversationId))
                .SendAsync("message_received", message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish message_received for conversation {ConversationId}", conversationId);
        }
    }

    public async Task NotifyConversationUpdatedAsync(Guid userId, ConversationListItemDto item, CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.Group(ChatHub.BuildUserGroupName(userId))
                .SendAsync("conversation_updated", item, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish conversation_updated for user {UserId}", userId);
        }
    }

    public async Task NotifyMessagesReadAsync(Guid conversationId, Guid userId, MessagesReadDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.Group(ChatHub.BuildConversationGroupName(conversationId))
                .SendAsync("messages_read", dto, cancellationToken);

            await hubContext.Clients.Group(ChatHub.BuildUserGroupName(userId))
                .SendAsync("messages_read", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish messages_read for conversation {ConversationId}", conversationId);
        }
    }
}
