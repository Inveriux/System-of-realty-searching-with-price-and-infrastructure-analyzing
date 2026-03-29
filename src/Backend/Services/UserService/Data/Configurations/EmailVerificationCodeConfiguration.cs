using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Models;

namespace UserService.Data.Configurations;

public sealed class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.ToTable("email_verification_codes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.UserId, x.IsUsed, x.ExpiresAtUtc });
    }
}
