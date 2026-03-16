using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using Microsoft.Extensions.Caching.Memory;
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
    /// GraphQL HTTP client, the caching decorator, and the search query handler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method configures an <see cref="HttpClient"/> targeting the AniList GraphQL
    /// endpoint at <c>https://graphql.anilist.co</c>. It also registers
    /// <see cref="CachedAniListClient"/> as the <see cref="IAniListClient"/> implementation,
    /// wrapping the underlying <see cref="AniListGraphQlClient"/> with an in-memory cache
    /// to reduce API calls.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to add the AniListSync services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddAniListSyncModule(this IServiceCollection services)
    {
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

        services.AddScoped<SearchAnimeQueryHandler>();

        return services;
    }
}
