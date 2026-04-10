using ChatService.DTOs.Chat;
using ChatService.Models;

namespace ChatService.UseCases.Chat;

internal static class ChatMapping
{
    public static string GetConversationType(this Conversation conversation)
        => conversation.ListingId == Guid.Empty ? ConversationTypes.Support : ConversationTypes.Listing;

    public static string BuildUserPairKey(Guid firstUserId, Guid secondUserId)
    {
        var left = firstUserId.ToString("N");
        var right = secondUserId.ToString("N");
        return string.CompareOrdinal(left, right) <= 0 ? $"{left}_{right}" : $"{right}_{left}";
    }

    public static string BuildLastMessagePreview(string text)
    {
        var normalized = string.Join(" ", text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return normalized.Length <= 160 ? normalized : $"{normalized[..157]}...";
    }

    public static MessageDto ToDto(this ChatMessage message, Guid currentUserId)
        => new()
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            Text = message.Text,
            CreatedAt = message.CreatedAtUtc,
            IsRead = message.IsRead,
            IsMine = message.SenderUserId == currentUserId
        };

    public static string ResolveOtherUserDisplayName(this Conversation conversation, ConversationParticipant otherParticipant, string currentUserRole)
    {
        if (conversation.GetConversationType() == ConversationTypes.Support
            && !string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Поддержка Domio";
        }

        return otherParticipant.DisplayName;
    }

    public static ConversationDetailsDto ToDetailsDto(this Conversation conversation, Guid currentUserId, string currentUserRole)
    {
        var other = conversation.Participants.First(x => x.UserId != currentUserId);
        return new ConversationDetailsDto
        {
            ConversationId = conversation.Id,
            ConversationType = conversation.GetConversationType(),
            ListingId = conversation.ListingId,
            ListingTitle = conversation.ListingTitle,
            ListingPrice = conversation.ListingPriceAmount,
            ListingPriceCurrency = conversation.ListingPriceCurrency,
            ListingImageUrl = conversation.ListingImageUrl,
            OtherUserId = other.UserId,
            OtherUserName = conversation.ResolveOtherUserDisplayName(other, currentUserRole),
            OtherUserAvatarUrl = other.AvatarUrl,
            OtherUserEmail = null,
            OtherUserPhone = null
        };
    }
}
