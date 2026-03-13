using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.From(value))
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(DisplayName.MaxLength)
            .HasConversion(
                dn => dn.Value,
                value => DisplayName.From(value))
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.AccountStatus)
            .HasColumnName("account_status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(u => u.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .IsRequired();

        builder.Property(u => u.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
    }
}
