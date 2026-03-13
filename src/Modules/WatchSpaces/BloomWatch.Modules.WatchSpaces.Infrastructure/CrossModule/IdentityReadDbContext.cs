using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;

/// <summary>
/// Minimal read-only projection of identity.users for cross-schema lookups.
/// Only used by WatchSpaces infrastructure — never written to.
/// </summary>
public sealed class IdentityReadDbContext(DbContextOptions<IdentityReadDbContext> options) : DbContext(options)
{
    public DbSet<IdentityUserRow> Users => Set<IdentityUserRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserRow>(entity =>
        {
            entity.ToTable("users", "identity");
            entity.HasNoKey();
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.DisplayName).HasColumnName("display_name");
        });
    }
}

public sealed class IdentityUserRow
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
