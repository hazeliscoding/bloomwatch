using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Extensions;

/// <summary>
/// Registers the AnimeTracking module's services and cross-module read contexts
/// into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the AnimeTracking module to the service collection, including the primary
    /// <see cref="AnimeTrackingDbContext"/>, cross-module read contexts for WatchSpaces
    /// membership and AniListSync media cache, and all repository/abstraction implementations.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration (requires <c>ConnectionStrings:DefaultConnection</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
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

        return services;
    }
}
