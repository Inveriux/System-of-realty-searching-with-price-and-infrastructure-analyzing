namespace ChatService.DTOs.Chat;

public sealed class ConversationListItemDto
{
    public Guid ConversationId { get; init; }
    public string ConversationType { get; init; } = string.Empty;
    public Guid ListingId { get; init; }
    public string ListingTitle { get; init; } = string.Empty;
    public decimal? ListingPrice { get; init; }
    public string? ListingPriceCurrency { get; init; }
    public string? ListingImageUrl { get; init; }
    public Guid OtherUserId { get; init; }
    public string OtherUserName { get; init; } = string.Empty;
    public string? OtherUserAvatarUrl { get; init; }
    public string? LastMessagePreview { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
}

public sealed class ConversationDetailsDto
{
    public Guid ConversationId { get; init; }
    public string ConversationType { get; init; } = string.Empty;
    public Guid ListingId { get; init; }
    public string ListingTitle { get; init; } = string.Empty;
    public decimal? ListingPrice { get; init; }
    public string? ListingPriceCurrency { get; init; }
    public string? ListingImageUrl { get; init; }
    public Guid OtherUserId { get; init; }
    public string OtherUserName { get; init; } = string.Empty;
    public string? OtherUserAvatarUrl { get; init; }
    public string? OtherUserEmail { get; init; }
    public string? OtherUserPhone { get; init; }
}

public sealed class MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderUserId { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public bool IsRead { get; init; }
    public bool IsMine { get; init; }
}

public sealed class ConversationMessagesPageDto
{
    public IReadOnlyCollection<MessageDto> Messages { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore { get; init; }
}

public sealed class UnreadCountDto
{
    public int Count { get; init; }
}

public sealed class MessagesReadDto
{
    public Guid ConversationId { get; init; }
    public Guid ReaderUserId { get; init; }
    public IReadOnlyCollection<Guid> MessageIds { get; init; } = [];
    public DateTime ReadAt { get; init; }
}
