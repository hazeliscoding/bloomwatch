using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using BloomWatch.Modules.AniListSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.AniListSync.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering the AniListSync module's services
/// in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services required by the AniListSync module, including the AniList
    /// GraphQL HTTP client, the caching decorator, persistence, and query handlers.
    /// </summary>
    /// <param name="services">The service collection to add the AniListSync services to.</param>
    /// <param name="configuration">The application configuration for reading connection strings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddAniListSyncModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Persistence
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<AniListSyncDbContext>(options =>
            options.UseNpgsql(dataSource));

        services.AddScoped<IMediaCacheRepository, EfMediaCacheRepository>();

        // AniList HTTP client + caching decorator
        services.AddMemoryCache();

        services.AddHttpClient<AniListGraphQlClient>(client =>
        {
            client.BaseAddress = new Uri("https://graphql.anilist.co");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddScoped<IAniListClient>(sp =>
            new CachedAniListClient(
                sp.GetRequiredService<AniListGraphQlClient>(),
                sp.GetRequiredService<IMemoryCache>()));

        return services;
    }
}
