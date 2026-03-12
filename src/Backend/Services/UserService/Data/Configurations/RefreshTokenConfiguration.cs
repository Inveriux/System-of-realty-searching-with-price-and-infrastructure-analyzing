using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithOne(x => x.RefreshToken)
            .HasForeignKey<RefreshToken>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}