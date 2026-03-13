using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres")
            .Options;

        return new IdentityDbContext(options);
    }
}
