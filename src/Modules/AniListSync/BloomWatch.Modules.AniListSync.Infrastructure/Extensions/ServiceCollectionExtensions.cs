using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using BloomWatch.Modules.AniListSync.Infrastructure.AniList;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.AniListSync.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
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
