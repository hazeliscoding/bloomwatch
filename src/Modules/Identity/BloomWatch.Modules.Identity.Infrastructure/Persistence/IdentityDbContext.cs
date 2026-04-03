using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core <see cref="DbContext"/> for the Identity module.
/// </summary>
/// <remarks>
/// <para>
/// All tables are placed in the <c>identity</c> database schema to isolate Identity data from other
/// modules in the same physical database (schema-per-module strategy).
/// </para>
/// <para>
/// Entity configurations are applied via <see cref="UserConfiguration"/> in
/// <see cref="OnModelCreating"/>. Add additional <c>IEntityTypeConfiguration&lt;T&gt;</c>
/// implementations there as the domain grows.
/// </para>
/// </remarks>
/// <param name="options">The EF Core options for this context, typically configured with a PostgreSQL provider.</param>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    /// <summary>Gets the <see cref="DbSet{TEntity}"/> for <see cref="User"/> aggregates.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Gets the <see cref="DbSet{TEntity}"/> for <see cref="RefreshToken"/> entities.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Gets the <see cref="DbSet{TEntity}"/> for <see cref="PasswordResetToken"/> entities.</summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    /// <summary>
    /// Configures the entity model for the Identity module.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
    }
}
