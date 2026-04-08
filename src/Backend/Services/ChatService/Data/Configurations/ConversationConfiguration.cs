using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Data.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversation");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserPairKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ListingTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ListingPriceCurrency).HasMaxLength(10);
        builder.Property(x => x.ListingImageUrl).HasMaxLength(1000);
        builder.Property(x => x.LastMessagePreview).HasMaxLength(500);
        builder.Property(x => x.ListingPriceAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.LastMessageAtUtc);
        builder.HasIndex(x => new { x.ListingId, x.UserPairKey }).IsUnique();

        builder.HasMany(x => x.Participants)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
