using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;

internal sealed class WatchSpacesDbContextFactory : IDesignTimeDbContextFactory<WatchSpacesDbContext>
{
    public WatchSpacesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WatchSpacesDbContext>()
            .UseNpgsql("Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres")
            .Options;

        return new WatchSpacesDbContext(options);
    }
}
