namespace ChatService.Models;

public sealed class Conversation
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public decimal? ListingPriceAmount { get; set; }
    public string? ListingPriceCurrency { get; set; }
    public string? ListingImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? LastMessageAtUtc { get; set; }
    public string? LastMessagePreview { get; set; }
    public Guid? LastSenderUserId { get; set; }
    public string UserPairKey { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<ChatMessage> Messages { get; set; } = [];
}
