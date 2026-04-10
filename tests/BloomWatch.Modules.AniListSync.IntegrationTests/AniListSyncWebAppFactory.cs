using System.Net;
using System.Text.Json;
using BloomWatch.Modules.AniListSync.Domain.Entities;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using BloomWatch.Modules.AniListSync.Infrastructure.Persistence;
using BloomWatch.Modules.Identity.Infrastructure.Persistence;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.AniListSync.IntegrationTests;

/// <summary>
/// An <see cref="IModelCustomizer"/> that wraps the default EF Core customizer and then
/// patches any <c>jsonb</c>-typed properties to use <c>TEXT</c> with JSON value converters,
/// making the model compatible with SQLite in integration tests.
/// </summary>
internal sealed class SqliteJsonModelCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        SqliteJsonPatch.ApplyToModel(modelBuilder);
    }
}

/// <summary>
/// Patches the EF Core model to replace <c>jsonb</c> column type annotations with
/// plain <c>TEXT</c> + JSON value converters so that SQLite can validate and use them.
/// </summary>
internal static class SqliteJsonPatch
{
    public static void ApplyToModel(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.GetColumnType() != "jsonb")
                    continue;

                var clrType = property.ClrType;
                property.SetColumnType("TEXT");

                if (clrType == typeof(IReadOnlyList<string>))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<IReadOnlyList<string>, string>(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => (IReadOnlyList<string>)(JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())));
                }
                else if (clrType == typeof(IReadOnlyList<MediaTag>))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<IReadOnlyList<MediaTag>, string>(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => (IReadOnlyList<MediaTag>)(JsonSerializer.Deserialize<List<MediaTag>>(v, (JsonSerializerOptions?)null) ?? new List<MediaTag>())));
                }
            }
        }
    }
}

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
            // Replace DbContexts with SQLite in-memory. AniListSyncDbContext uses jsonb
            // columns, so we also replace the IModelCustomizer service to patch those
            // column types to TEXT + JSON value converters for SQLite compatibility.
            ReplaceDbContext<IdentityDbContext>(services, _identityConnection!);
            ReplaceDbContext<WatchSpacesDbContext>(services, _watchSpacesConnection!);
            ReplaceDbContext<AniListSyncDbContext>(services, _aniListSyncConnection!,
                patchJsonb: true);

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

    private static void ReplaceDbContext<TContext>(
        IServiceCollection services,
        SqliteConnection connection,
        bool patchJsonb = false)
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

        if (patchJsonb)
        {
            services.AddDbContext<TContext>(options =>
                options.UseSqlite(connection)
                       .ReplaceService<IModelCustomizer, SqliteJsonModelCustomizer>());
        }
        else
        {
            services.AddDbContext<TContext>(options => options.UseSqlite(connection));
        }
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
