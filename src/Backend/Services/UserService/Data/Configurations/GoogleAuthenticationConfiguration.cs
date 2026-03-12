using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data.Configurations;

public class GoogleAuthenticationConfiguration : IEntityTypeConfiguration<GoogleAuthentication>
{
    private const string TableName = "google_authentications";

    public void Configure(EntityTypeBuilder<GoogleAuthentication> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.HasOne<User>()
            .WithOne(u => u.GoogleAuthentication)
            .HasForeignKey<GoogleAuthentication>(g => g.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(g => g.GoogleId)
            .IsRequired();
    }
}