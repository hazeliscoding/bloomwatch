using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="IdentityDbContext"/>, used by EF Core tooling
/// (e.g., <c>dotnet ef migrations add</c>) when no runtime host is available.
/// </summary>
/// <remarks>
/// The connection string is hard-coded to a local PostgreSQL development instance.
/// This factory is never used at runtime; the runtime context is configured through
/// <see cref="Extensions.ServiceCollectionExtensions.AddIdentityModule"/>.
/// </remarks>
internal sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    /// <summary>
    /// Creates a new <see cref="IdentityDbContext"/> configured with a local PostgreSQL connection string.
    /// </summary>
    /// <param name="args">Command-line arguments passed by the EF Core tools (currently unused).</param>
    /// <returns>A configured <see cref="IdentityDbContext"/> instance for design-time operations.</returns>
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres")
            .Options;

        return new IdentityDbContext(options);
    }
}
