using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="Invitation"/> entity.
/// </summary>
/// <remarks>
/// <para>Maps to the <c>watch_spaces.invitations</c> table with the following columns:</para>
/// <list type="bullet">
///   <item><description><c>id</c> (PK) -- <see cref="Guid"/>.</description></item>
///   <item><description><c>watch_space_id</c> -- required FK to <c>watch_spaces</c>, stored as <see cref="Guid"/> with <see cref="WatchSpaceId"/> conversion.</description></item>
///   <item><description><c>invited_by_user_id</c> -- required <see cref="Guid"/>.</description></item>
///   <item><description><c>invited_email</c> -- required, max 320 characters (RFC 5321 maximum).</description></item>
///   <item><description><c>token</c> -- required, max 64 characters, unique invitation acceptance token.</description></item>
///   <item><description><c>status</c> -- required, stored as a string enum conversion.</description></item>
///   <item><description><c>expires_at_utc</c> -- required expiration timestamp.</description></item>
///   <item><description><c>created_at_utc</c> -- required creation timestamp.</description></item>
///   <item><description><c>accepted_at_utc</c> -- nullable timestamp set when the invitation is accepted.</description></item>
/// </list>
/// <para>Indexes:</para>
/// <list type="bullet">
///   <item><description><c>ix_invitations_token</c> -- unique index on <c>token</c> for fast lookup during acceptance.</description></item>
/// </list>
/// </remarks>
internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    /// <summary>
    /// Configures the <see cref="Invitation"/> entity mapping, column definitions, and indexes.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Invitation"/>.</param>
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.WatchSpaceId)
            .HasColumnName("watch_space_id")
            .HasConversion(id => id.Value, value => WatchSpaceId.From(value))
            .IsRequired();

        builder.Property(i => i.InvitedByUserId)
            .HasColumnName("invited_by_user_id")
            .IsRequired();

        builder.Property(i => i.InvitedEmail)
            .HasColumnName("invited_email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(i => i.Token)
            .HasColumnName("token")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(i => i.Token)
            .IsUnique()
            .HasDatabaseName("ix_invitations_token");

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(i => i.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(i => i.AcceptedAtUtc)
            .HasColumnName("accepted_at_utc");
    }
}
