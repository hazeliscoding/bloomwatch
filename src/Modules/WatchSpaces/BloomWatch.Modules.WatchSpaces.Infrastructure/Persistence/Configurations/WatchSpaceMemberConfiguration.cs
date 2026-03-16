using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="WatchSpaceMember"/> entity.
/// </summary>
/// <remarks>
/// <para>Maps to the <c>watch_spaces.watch_space_members</c> table with the following columns:</para>
/// <list type="bullet">
///   <item><description><c>id</c> (PK) -- <see cref="Guid"/>.</description></item>
///   <item><description><c>watch_space_id</c> -- required FK to <c>watch_spaces</c>, stored as <see cref="Guid"/> with <see cref="WatchSpaceId"/> conversion.</description></item>
///   <item><description><c>user_id</c> -- required <see cref="Guid"/> identifying the member.</description></item>
///   <item><description><c>role</c> -- required, stored as a string enum conversion.</description></item>
///   <item><description><c>joined_at_utc</c> -- required timestamp.</description></item>
/// </list>
/// <para>Indexes:</para>
/// <list type="bullet">
///   <item><description><c>ix_watch_space_members_space_user</c> -- unique composite index on (<c>watch_space_id</c>, <c>user_id</c>) preventing duplicate memberships.</description></item>
/// </list>
/// </remarks>
internal sealed class WatchSpaceMemberConfiguration : IEntityTypeConfiguration<WatchSpaceMember>
{
    /// <summary>
    /// Configures the <see cref="WatchSpaceMember"/> entity mapping, column definitions, and indexes.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="WatchSpaceMember"/>.</param>
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
