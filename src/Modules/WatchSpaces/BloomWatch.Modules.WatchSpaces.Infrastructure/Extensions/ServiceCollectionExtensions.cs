using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Email;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering the WatchSpaces module's
/// infrastructure services into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all WatchSpaces module services including persistence, repositories,
    /// cross-module read models, email senders, integration event publishers, and
    /// command/query handlers.
    /// </summary>
    /// <remarks>
    /// <para>This method registers the following infrastructure concerns:</para>
    /// <list type="bullet">
    ///   <item><description><see cref="WatchSpacesDbContext"/> -- the module's primary EF Core context (PostgreSQL, <c>watch_spaces</c> schema).</description></item>
    ///   <item><description><see cref="IdentityReadDbContext"/> -- read-only cross-module context for <c>identity.users</c> lookups.</description></item>
    ///   <item><description><see cref="EfWatchSpaceRepository"/> as <see cref="IWatchSpaceRepository"/>.</description></item>
    ///   <item><description><see cref="UserReadModel"/> as <see cref="IUserReadModel"/>.</description></item>
    ///   <item><description><see cref="NoOpInvitationEmailSender"/> as <see cref="IInvitationEmailSender"/> (development placeholder).</description></item>
    ///   <item><description><see cref="InProcessIntegrationEventPublisher"/> as <see cref="IIntegrationEventPublisher"/> (development placeholder).</description></item>
    ///   <item><description>All command and query handlers for the module's use cases.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configuration">
    /// Application configuration; must contain a <c>ConnectionStrings:DefaultConnection</c> entry
    /// pointing to the PostgreSQL database.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddWatchSpacesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Persistence
        services.AddDbContext<WatchSpacesDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Cross-module read model (read-only access to identity.users)
        services.AddDbContext<IdentityReadDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IWatchSpaceRepository, EfWatchSpaceRepository>();

        // Abstractions
        services.AddScoped<IUserReadModel, UserReadModel>();
        services.AddScoped<IUserDisplayNameLookup, UserDisplayNameLookup>();

        var smtpHost = configuration["Email:Smtp:Host"];
        if (!string.IsNullOrEmpty(smtpHost))
            services.AddScoped<IInvitationEmailSender, SmtpInvitationEmailSender>();
        else
            services.AddScoped<IInvitationEmailSender, NoOpInvitationEmailSender>();

        services.AddScoped<IIntegrationEventPublisher, InProcessIntegrationEventPublisher>();

        return services;
    }
}
