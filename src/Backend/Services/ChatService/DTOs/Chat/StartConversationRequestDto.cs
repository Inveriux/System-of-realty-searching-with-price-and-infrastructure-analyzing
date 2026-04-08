namespace ChatService.DTOs.Chat;

public sealed class StartConversationRequestDto
{
    public Guid ListingId { get; init; }
    public Guid? OtherUserId { get; init; }
}
