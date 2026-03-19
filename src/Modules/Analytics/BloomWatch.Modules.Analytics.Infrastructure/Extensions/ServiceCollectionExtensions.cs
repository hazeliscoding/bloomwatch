using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;
using BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;
using BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;
using BloomWatch.Modules.Analytics.Infrastructure.CrossModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.Analytics.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
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

        // Query handlers
        services.AddScoped<GetDashboardSummaryQueryHandler>();
        services.AddScoped<GetCompatibilityQueryHandler>();
        services.AddScoped<GetRatingGapsQueryHandler>();
        services.AddScoped<GetSharedStatsQueryHandler>();
        services.AddScoped<GetRandomPickQueryHandler>();

        return services;
    }
}
