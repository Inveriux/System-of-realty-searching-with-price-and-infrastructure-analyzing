using ChatService.DTOs.Chat;
using ChatService.Models;

namespace ChatService.UseCases.Chat;

internal static class ChatMapping
{
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

    public static ConversationDetailsDto ToDetailsDto(this Conversation conversation, Guid currentUserId)
    {
        var other = conversation.Participants.First(x => x.UserId != currentUserId);
        return new ConversationDetailsDto
        {
            ConversationId = conversation.Id,
            ListingId = conversation.ListingId,
            ListingTitle = conversation.ListingTitle,
            ListingPrice = conversation.ListingPriceAmount,
            ListingPriceCurrency = conversation.ListingPriceCurrency,
            ListingImageUrl = conversation.ListingImageUrl,
            OtherUserId = other.UserId,
            OtherUserName = other.DisplayName,
            OtherUserAvatarUrl = other.AvatarUrl
        };
    }
}
