namespace ChatService.Models;

public sealed class ConversationParticipant
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public Conversation Conversation { get; set; } = null!;
}
