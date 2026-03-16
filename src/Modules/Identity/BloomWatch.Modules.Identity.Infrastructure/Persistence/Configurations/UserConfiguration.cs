using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Entity Framework Core mapping for the <see cref="User"/> aggregate root.
/// </summary>
/// <remarks>
/// <para>Maps to the <c>identity.users</c> table with the following columns and constraints:</para>
/// <list type="table">
///   <listheader>
///     <term>Column</term>
///     <description>Details</description>
///   </listheader>
///   <item>
///     <term><c>user_id</c> (PK)</term>
///     <description><see cref="UserId"/> value object, converted to/from <see cref="Guid"/>.</description>
///   </item>
///   <item>
///     <term><c>email</c></term>
///     <description><see cref="EmailAddress"/> value object, max 320 chars, unique index (<c>ix_users_email</c>).</description>
///   </item>
///   <item>
///     <term><c>display_name</c></term>
///     <description><see cref="DisplayName"/> value object, max length enforced by <see cref="DisplayName.MaxLength"/>.</description>
///   </item>
///   <item>
///     <term><c>password_hash</c></term>
///     <description>Bcrypt hash string, max 100 chars.</description>
///   </item>
///   <item>
///     <term><c>account_status</c></term>
///     <description>Enum stored as a string (e.g., <c>"Active"</c>, <c>"Suspended"</c>).</description>
///   </item>
///   <item>
///     <term><c>is_email_verified</c></term>
///     <description>Boolean flag.</description>
///   </item>
///   <item>
///     <term><c>created_at_utc</c></term>
///     <description>UTC timestamp of account creation.</description>
///   </item>
/// </list>
/// </remarks>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Applies the <see cref="User"/> entity mapping to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the <see cref="User"/> entity type.</param>
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
