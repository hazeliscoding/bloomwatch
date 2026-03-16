using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="DbContext"/> for the WatchSpaces module, targeting the
/// <c>watch_spaces</c> PostgreSQL schema.
/// </summary>
/// <remarks>
/// <para>This context owns three tables:</para>
/// <list type="bullet">
///   <item><description><c>watch_spaces.watch_spaces</c> -- the <see cref="WatchSpace"/> aggregate root.</description></item>
///   <item><description><c>watch_spaces.watch_space_members</c> -- membership child entities.</description></item>
///   <item><description><c>watch_spaces.invitations</c> -- pending/accepted/declined invitation child entities.</description></item>
/// </list>
/// <para>
/// Entity configurations are applied via <see cref="WatchSpaceConfiguration"/>,
/// <see cref="WatchSpaceMemberConfiguration"/>, and <see cref="InvitationConfiguration"/>.
/// </para>
/// </remarks>
/// <param name="options">The EF Core options configured with the PostgreSQL connection string.</param>
public sealed class WatchSpacesDbContext(DbContextOptions<WatchSpacesDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the queryable set of <see cref="WatchSpace"/> aggregate roots.
    /// </summary>
    public DbSet<WatchSpace> WatchSpaces => Set<WatchSpace>();

    /// <summary>
    /// Applies the default <c>watch_spaces</c> schema and all entity type configurations.
    /// </summary>
    /// <param name="modelBuilder">The model builder provided by EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("watch_spaces");
        modelBuilder.ApplyConfiguration(new WatchSpaceConfiguration());
        modelBuilder.ApplyConfiguration(new WatchSpaceMemberConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationConfiguration());
    }
}
