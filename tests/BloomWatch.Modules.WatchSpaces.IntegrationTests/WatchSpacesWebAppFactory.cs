using BloomWatch.Modules.Identity.Infrastructure.Persistence;
using BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.WatchSpaces.IntegrationTests;

/// <summary>
/// Uses two named SQLite in-memory databases with shared cache so that each
/// DbContext has its own clean database to call EnsureCreated against:
/// - "identity_db": IdentityDbContext + IdentityReadDbContext
/// - "watchspaces_db": WatchSpacesDbContext
/// </summary>
public sealed class WatchSpacesWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _identityConnection;
    private SqliteConnection? _watchSpacesConnection;
    private readonly string _dbSuffix = Guid.NewGuid().ToString("N");

    public async Task InitializeAsync()
    {
        _identityConnection = new SqliteConnection($"Data Source=identity_{_dbSuffix};Mode=Memory;Cache=Shared");
        _watchSpacesConnection = new SqliteConnection($"Data Source=watchspaces_{_dbSuffix};Mode=Memory;Cache=Shared");

        await _identityConnection.OpenAsync();
        await _watchSpacesConnection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_identityConnection is not null)
            await _identityConnection.DisposeAsync();
        if (_watchSpacesConnection is not null)
            await _watchSpacesConnection.DisposeAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            RemoveDbContext<IdentityDbContext>(services);
            RemoveDbContext<WatchSpacesDbContext>(services);
            RemoveDbContext<IdentityReadDbContext>(services);

            // Identity + cross-module read share the identity connection
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlite(_identityConnection!));
            services.AddDbContext<IdentityReadDbContext>(options =>
                options.UseSqlite(_identityConnection!));

            // WatchSpaces has its own connection
            services.AddDbContext<WatchSpacesDbContext>(options =>
                options.UseSqlite(_watchSpacesConnection!));
        });

        builder.UseEnvironment("Testing");
    }

    public void EnsureSchemaCreated()
    {
        using var scope = Services.CreateScope();

        // Each context calls EnsureCreated on its own clean database
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<WatchSpacesDbContext>().Database.EnsureCreated();
    }

    private static void RemoveDbContext<T>(IServiceCollection services) where T : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<T>) ||
                d.ServiceType == typeof(T) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition().Name.Contains("DbContextOptionsConfiguration") &&
                 d.ServiceType.GenericTypeArguments.Length == 1 &&
                 d.ServiceType.GenericTypeArguments[0] == typeof(T)))
            .ToList();

        foreach (var descriptor in toRemove)
            services.Remove(descriptor);
    }
}
