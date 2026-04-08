using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using ChatService.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record MarkConversationReadCommand(Guid ConversationId) : IRequest<Result>;

public sealed class MarkConversationReadHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IChatRealtimeNotifier chatRealtimeNotifier) : IRequestHandler<MarkConversationReadCommand, Result>
{
    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var conversation = await dbContext.Conversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(
                x => x.Id == request.ConversationId && x.Participants.Any(p => p.UserId == currentUserId),
                cancellationToken);

        if (conversation is null)
        {
            return Result.NotFound("Диалог не найден.");
        }

        var unreadMessages = await dbContext.Messages
            .Where(x => x.ConversationId == request.ConversationId && x.SenderUserId != currentUserId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadMessages.Count == 0)
        {
            return Result.Ok();
        }

        var readAt = DateTime.UtcNow;
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAtUtc = readAt;
            message.UpdatedAtUtc = readAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var otherParticipantId = conversation.Participants.First(x => x.UserId != currentUserId).UserId;
        var payload = new MessagesReadDto
        {
            ConversationId = conversation.Id,
            ReaderUserId = currentUserId,
            MessageIds = unreadMessages.Select(x => x.Id).ToArray(),
            ReadAt = readAt
        };

        await chatRealtimeNotifier.NotifyMessagesReadAsync(conversation.Id, otherParticipantId, payload, cancellationToken);

        var currentUserConversation = await BuildConversationListItemAsync(dbContext, conversation.Id, currentUserId, cancellationToken);
        var otherUserConversation = await BuildConversationListItemAsync(dbContext, conversation.Id, otherParticipantId, cancellationToken);

        if (currentUserConversation is not null)
        {
            await chatRealtimeNotifier.NotifyConversationUpdatedAsync(currentUserId, currentUserConversation, cancellationToken);
        }

        if (otherUserConversation is not null)
        {
            await chatRealtimeNotifier.NotifyConversationUpdatedAsync(otherParticipantId, otherUserConversation, cancellationToken);
        }

        return Result.Ok();
    }

    internal static async Task<ConversationListItemDto?> BuildConversationListItemAsync(
        ApplicationDbContext dbContext,
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(
                x => x.Id == conversationId && x.Participants.Any(p => p.UserId == userId),
                cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        var unreadCount = await dbContext.Messages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId && x.SenderUserId != userId && !x.IsRead)
            .CountAsync(cancellationToken);

        var other = conversation.Participants.First(x => x.UserId != userId);
        return new ConversationListItemDto
        {
            ConversationId = conversation.Id,
            ListingId = conversation.ListingId,
            ListingTitle = conversation.ListingTitle,
            ListingPrice = conversation.ListingPriceAmount,
            ListingPriceCurrency = conversation.ListingPriceCurrency,
            ListingImageUrl = conversation.ListingImageUrl,
            OtherUserId = other.UserId,
            OtherUserName = other.DisplayName,
            OtherUserAvatarUrl = other.AvatarUrl,
            LastMessagePreview = conversation.LastMessagePreview,
            LastMessageAt = conversation.LastMessageAtUtc,
            UnreadCount = unreadCount
        };
    }
}
