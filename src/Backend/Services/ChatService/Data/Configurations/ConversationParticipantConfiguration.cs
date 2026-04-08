using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Data.Configurations;

internal sealed class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("conversation_participant");

        builder.HasKey(x => new { x.ConversationId, x.UserId });

        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(1000);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ConversationId);
    }
}
