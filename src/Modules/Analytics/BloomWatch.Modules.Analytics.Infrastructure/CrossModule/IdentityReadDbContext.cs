using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Analytics.Infrastructure.CrossModule;

/// <summary>
/// Minimal read-only context projecting the <c>identity.users</c> table
/// for display name lookups.
/// </summary>
public sealed class IdentityReadDbContext(
    DbContextOptions<IdentityReadDbContext> options) : DbContext(options)
{
    public DbSet<IdentityUserRow> Users => Set<IdentityUserRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserRow>(entity =>
        {
            entity.ToTable("users", "identity");
            entity.HasNoKey();
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.DisplayName).HasColumnName("display_name");
        });
    }
}

/// <summary>
/// Read-only projection of a row from the <c>identity.users</c> table,
/// carrying only the fields needed for display name lookups.
/// </summary>
public sealed class IdentityUserRow
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
