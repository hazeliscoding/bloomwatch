using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;

internal sealed class WatchSpaceMemberConfiguration : IEntityTypeConfiguration<WatchSpaceMember>
{
    public void Configure(EntityTypeBuilder<WatchSpaceMember> builder)
    {
        builder.ToTable("watch_space_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.WatchSpaceId)
            .HasColumnName("watch_space_id")
            .HasConversion(id => id.Value, value => WatchSpaceId.From(value))
            .IsRequired();

        builder.Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(m => m.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.JoinedAtUtc)
            .HasColumnName("joined_at_utc")
            .IsRequired();

        builder.HasIndex(m => new { m.WatchSpaceId, m.UserId })
            .IsUnique()
            .HasDatabaseName("ix_watch_space_members_space_user");
    }
}
