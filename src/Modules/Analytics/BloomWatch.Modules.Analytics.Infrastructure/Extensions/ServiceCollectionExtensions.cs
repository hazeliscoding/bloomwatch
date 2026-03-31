using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Infrastructure.CrossModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.Analytics.Infrastructure.Extensions;

/// <summary>
/// Registers the Analytics module's services and cross-module read contexts
/// into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Analytics module to the service collection, including cross-module
    /// read-only <see cref="Microsoft.EntityFrameworkCore.DbContext"/> instances for
    /// AnimeTracking, WatchSpaces, and Identity data, plus the abstraction implementations.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration (requires <c>ConnectionStrings:DefaultConnection</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAnalyticsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Cross-module read contexts
        services.AddDbContext<AnimeTrackingReadDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddDbContext<WatchSpaceMembershipReadDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddDbContext<IdentityReadDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Abstractions
        services.AddScoped<IMembershipChecker, MembershipChecker>();
        services.AddScoped<IWatchSpaceAnalyticsDataSource, WatchSpaceAnalyticsDataSource>();
        services.AddScoped<IUserDisplayNameLookup, UserDisplayNameLookup>();

        return services;
    }
}
