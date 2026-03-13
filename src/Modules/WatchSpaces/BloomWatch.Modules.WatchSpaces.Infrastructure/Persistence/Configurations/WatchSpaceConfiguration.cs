using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;

internal sealed class WatchSpaceConfiguration : IEntityTypeConfiguration<WatchSpace>
{
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
