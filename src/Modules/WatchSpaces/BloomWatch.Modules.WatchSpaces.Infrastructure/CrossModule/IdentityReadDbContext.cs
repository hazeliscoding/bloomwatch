using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;

/// <summary>
/// Minimal read-only projection of the <c>identity.users</c> table for cross-schema lookups.
/// </summary>
/// <remarks>
/// This context enables the WatchSpaces module to resolve user details (email, display name)
/// owned by the Identity module without introducing a runtime dependency on Identity services.
/// It is strictly read-only and must never be used to write to the Identity schema.
/// Maps to the <c>identity.users</c> table using a keyless entity (<see cref="IdentityUserRow"/>).
/// </remarks>
/// <param name="options">The EF Core options configured with the shared PostgreSQL connection string.</param>
public sealed class IdentityReadDbContext(DbContextOptions<IdentityReadDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the queryable set of identity user rows for read-only lookups.
    /// </summary>
    public DbSet<IdentityUserRow> Users => Set<IdentityUserRow>();

    /// <summary>
    /// Configures the keyless <see cref="IdentityUserRow"/> entity to map to the
    /// <c>identity.users</c> table with snake_case column names.
    /// </summary>
    /// <param name="modelBuilder">The model builder provided by EF Core.</param>
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

/// <summary>
/// Read-only row projection of a user from the <c>identity.users</c> table.
/// </summary>
/// <remarks>
/// This is a keyless entity used exclusively for cross-module queries. It carries only the
/// columns the WatchSpaces module needs and must not be used for persistence operations.
/// </remarks>
public sealed class IdentityUserRow
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's display name.</summary>
    public string DisplayName { get; set; } = string.Empty;
}
