using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="WatchSpace"/> aggregate root.
/// </summary>
/// <remarks>
/// <para>Maps to the <c>watch_spaces.watch_spaces</c> table with the following columns:</para>
/// <list type="bullet">
///   <item><description><c>id</c> (PK) -- <see cref="WatchSpaceId"/> value object, stored as <see cref="Guid"/>.</description></item>
///   <item><description><c>name</c> -- required, max 100 characters.</description></item>
///   <item><description><c>created_by_user_id</c> -- required <see cref="Guid"/>.</description></item>
///   <item><description><c>created_at_utc</c>, <c>updated_at_utc</c> -- required timestamps.</description></item>
/// </list>
/// <para>Relationships:</para>
/// <list type="bullet">
///   <item><description>One-to-many with <see cref="WatchSpaceMember"/> (cascade delete), navigated via backing field <c>_members</c>.</description></item>
///   <item><description>One-to-many with <see cref="Invitation"/> (cascade delete), navigated via backing field <c>_invitations</c>.</description></item>
/// </list>
/// </remarks>
internal sealed class WatchSpaceConfiguration : IEntityTypeConfiguration<WatchSpace>
{
    /// <summary>
    /// Configures the <see cref="WatchSpace"/> entity mapping, column definitions, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="WatchSpace"/>.</param>
    public void Configure(EntityTypeBuilder<WatchSpace> builder)
    {
        builder.ToTable("watch_spaces");

        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => WatchSpaceId.From(value));

        builder.Property(ws => ws.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ws => ws.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(ws => ws.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(ws => ws.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        // Use the public property as the navigation name but access via backing field.
        // This allows LINQ queries (ws.Members.Any(...)) to translate to SQL correctly,
        // and ensures EF Core change tracking works when modifying child entities.
        builder.HasMany(ws => ws.Members)
            .WithOne()
            .HasForeignKey(m => m.WatchSpaceId)
            .HasPrincipalKey(ws => ws.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ws => ws.Invitations)
            .WithOne()
            .HasForeignKey(i => i.WatchSpaceId)
            .HasPrincipalKey(ws => ws.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(ws => ws.Members)
            .HasField("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(ws => ws.Invitations)
            .HasField("_invitations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
