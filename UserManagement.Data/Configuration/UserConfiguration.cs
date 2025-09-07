using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Data.Entities;

namespace UserManagement.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Forename)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Surname)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasMany(u => u.Logs)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId);

        builder.HasIndex(e => e.IsActive);
    }
}
