using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Data.Configurations;

internal sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("message");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ClientMessageId).HasMaxLength(100);

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.ConversationId, x.IsRead });
    }
}
