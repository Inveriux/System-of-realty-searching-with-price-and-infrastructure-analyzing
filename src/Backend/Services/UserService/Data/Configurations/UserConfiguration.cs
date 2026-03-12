using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const int MaxNameLength = 100;
    private const int MaxUserNameLength = 256;

    private const string TableName = "users";


    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableName);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(MaxNameLength);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(MaxUserNameLength);

        builder.HasOne(u => u.GoogleAuthentication)
            .WithOne(g => g.User)
            .HasForeignKey<GoogleAuthentication>(g => g.Id);
        
        builder.HasOne(x => x.RefreshToken)
            .WithOne(x => x.User)
            .HasForeignKey<RefreshToken>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
