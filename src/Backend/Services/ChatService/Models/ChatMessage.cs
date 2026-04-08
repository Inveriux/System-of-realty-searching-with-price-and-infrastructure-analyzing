namespace ChatService.Models;

public sealed class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public string? ClientMessageId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Conversation Conversation { get; set; } = null!;
}
