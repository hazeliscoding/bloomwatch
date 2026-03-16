using System.Net;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using BloomWatch.Modules.AniListSync.Infrastructure.Persistence;
using BloomWatch.Modules.Identity.Infrastructure.Persistence;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.AniListSync.IntegrationTests;

public sealed class AniListSyncWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _identityConnection;
    private SqliteConnection? _watchSpacesConnection;
    private SqliteConnection? _aniListSyncConnection;

    private StubAniListHandler _stubHandler = new();

    public StubAniListHandler StubHandler => _stubHandler;

    public async Task InitializeAsync()
    {
        _identityConnection = new SqliteConnection("Data Source=:memory:");
        await _identityConnection.OpenAsync();

        _watchSpacesConnection = new SqliteConnection("Data Source=:memory:");
        await _watchSpacesConnection.OpenAsync();

        _aniListSyncConnection = new SqliteConnection("Data Source=:memory:");
        await _aniListSyncConnection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_identityConnection is not null)
            await _identityConnection.DisposeAsync();
        if (_watchSpacesConnection is not null)
            await _watchSpacesConnection.DisposeAsync();
        if (_aniListSyncConnection is not null)
            await _aniListSyncConnection.DisposeAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContexts with SQLite in-memory
            ReplaceDbContext<IdentityDbContext>(services, _identityConnection!);
            ReplaceDbContext<WatchSpacesDbContext>(services, _watchSpacesConnection!);
            ReplaceDbContext<AniListSyncDbContext>(services, _aniListSyncConnection!);

            // Replace the AniListGraphQlClient's HttpClient with a stub handler
            services.AddHttpClient<AniListGraphQlClient>()
                .ConfigurePrimaryHttpMessageHandler(() => _stubHandler);
        });

        builder.UseEnvironment("Testing");
    }

    public void EnsureSchemaCreated()
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<WatchSpacesDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<AniListSyncDbContext>().Database.EnsureCreated();
    }

    private static void ReplaceDbContext<TContext>(IServiceCollection services, SqliteConnection connection)
        where TContext : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                d.ServiceType == typeof(TContext) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition().Name.Contains("DbContextOptionsConfiguration") &&
                 d.ServiceType.GenericTypeArguments.Length == 1 &&
                 d.ServiceType.GenericTypeArguments[0] == typeof(TContext)))
            .ToList();

        foreach (var descriptor in toRemove)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options => options.UseSqlite(connection));
    }
}

public sealed class StubAniListHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private string _responseBody = "{}";

    public void SetupSuccess(string responseJson)
    {
        _statusCode = HttpStatusCode.OK;
        _responseBody = responseJson;
    }

    public void SetupError(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        _responseBody = "";
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
