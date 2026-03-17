using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="AnimeTrackingDbContext"/>, used by EF Core CLI tools.
/// </summary>
internal sealed class AnimeTrackingDbContextFactory : IDesignTimeDbContextFactory<AnimeTrackingDbContext>
{
    public AnimeTrackingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AnimeTrackingDbContext>()
            .UseNpgsql("Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres")
            .Options;

        return new AnimeTrackingDbContext(options);
    }
}
