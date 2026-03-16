using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloomWatch.Modules.AniListSync.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="AniListSyncDbContext"/>, used by EF Core tooling
/// (e.g., <c>dotnet ef migrations add</c>) when no runtime host is available.
/// </summary>
/// <remarks>
/// The connection string is hard-coded to a local PostgreSQL development instance.
/// This factory is never used at runtime.
/// </remarks>
internal sealed class AniListSyncDbContextFactory : IDesignTimeDbContextFactory<AniListSyncDbContext>
{
    /// <inheritdoc />
    public AniListSyncDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AniListSyncDbContext>()
            .UseNpgsql("Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres")
            .Options;

        return new AniListSyncDbContext(options);
    }
}
