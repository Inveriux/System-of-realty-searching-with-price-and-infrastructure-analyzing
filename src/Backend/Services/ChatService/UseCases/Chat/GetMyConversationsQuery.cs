using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record GetMyConversationsQuery() : IRequest<Result<IReadOnlyCollection<ConversationListItemDto>>>;

public sealed class GetMyConversationsHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetMyConversationsQuery, Result<IReadOnlyCollection<ConversationListItemDto>>>
{
    public async Task<Result<IReadOnlyCollection<ConversationListItemDto>>> Handle(GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();

        var conversations = await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.Participants.Any(p => p.UserId == currentUserId))
            .Include(x => x.Participants)
            .OrderByDescending(x => x.LastMessageAtUtc ?? x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var conversationIds = conversations.Select(x => x.Id).ToArray();
        var unreadByConversation = conversationIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.Messages
                .AsNoTracking()
                .Where(x => conversationIds.Contains(x.ConversationId) && x.SenderUserId != currentUserId && !x.IsRead)
                .GroupBy(x => x.ConversationId)
                .Select(x => new { x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var items = conversations.Select(conversation =>
        {
            var other = conversation.Participants.First(x => x.UserId != currentUserId);
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
                UnreadCount = unreadByConversation.GetValueOrDefault(conversation.Id)
            };
        }).ToArray();

        return Result.Ok<IReadOnlyCollection<ConversationListItemDto>>(items);
    }
}
