namespace ChatService.DTOs.Chat;

public sealed class SendMessageRequestDto
{
    public string Text { get; init; } = string.Empty;
    public string? ClientMessageId { get; init; }
}
