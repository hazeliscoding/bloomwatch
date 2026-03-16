using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="WatchSpacesDbContext"/>, used by EF Core CLI
/// tools (e.g., <c>dotnet ef migrations add</c>) when no runtime host is available.
/// </summary>
/// <remarks>
/// The factory uses a hard-coded local development connection string. It is never
/// invoked at runtime -- the application host configures the context via DI instead.
/// </remarks>
internal sealed class WatchSpacesDbContextFactory : IDesignTimeDbContextFactory<WatchSpacesDbContext>
{
    /// <summary>
    /// Creates a new <see cref="WatchSpacesDbContext"/> configured with a local
    /// PostgreSQL connection string for design-time tooling.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded by the EF Core CLI (typically unused).</param>
    /// <returns>A configured <see cref="WatchSpacesDbContext"/> instance.</returns>
    public WatchSpacesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WatchSpacesDbContext>()
            .UseNpgsql("Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres")
            .Options;

        return new WatchSpacesDbContext(options);
    }
}
