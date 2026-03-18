using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.AddAnimeToWatchSpace;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnimeTrackingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Persistence
        services.AddDbContext<AnimeTrackingDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Cross-module read contexts
        services.AddDbContext<WatchSpaceMembershipReadDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddDbContext<AniListMediaCacheReadDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IAnimeTrackingRepository, EfAnimeTrackingRepository>();

        // Abstractions
        services.AddScoped<IMembershipChecker, MembershipChecker>();
        services.AddScoped<IMediaCacheLookup, MediaCacheLookup>();

        // Command handlers
        services.AddScoped<AddAnimeToWatchSpaceCommandHandler>();

        // Query handlers
        services.AddScoped<ListWatchSpaceAnimeQueryHandler>();

        return services;
    }
}
