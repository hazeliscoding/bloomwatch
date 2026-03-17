using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;

/// <summary>
/// Minimal read-only context projecting the <c>watch_spaces.watch_space_members</c> table
/// for membership checks.
/// </summary>
public sealed class WatchSpaceMembershipReadDbContext(
    DbContextOptions<WatchSpaceMembershipReadDbContext> options) : DbContext(options)
{
    public DbSet<WatchSpaceMemberRow> Members => Set<WatchSpaceMemberRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WatchSpaceMemberRow>(entity =>
        {
            entity.ToTable("watch_space_members", "watch_spaces");
            entity.HasNoKey();
            entity.Property(m => m.WatchSpaceId).HasColumnName("watch_space_id");
            entity.Property(m => m.UserId).HasColumnName("user_id");
        });
    }
}

/// <summary>
/// Read-only projection of watch_space_members for cross-module membership queries.
/// </summary>
public sealed class WatchSpaceMemberRow
{
    public Guid WatchSpaceId { get; set; }
    public Guid UserId { get; set; }
}
