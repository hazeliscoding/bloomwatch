using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
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
