using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.GetMyWatchSpaces;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.GetWatchSpaceById;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.LeaveWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.ListInvitations;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RemoveMember;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RevokeInvitation;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.TransferOwnership;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Email;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
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
        services.AddScoped<IInvitationEmailSender, NoOpInvitationEmailSender>();
        services.AddScoped<IIntegrationEventPublisher, InProcessIntegrationEventPublisher>();

        // Command handlers
        services.AddScoped<CreateWatchSpaceCommandHandler>();
        services.AddScoped<RenameWatchSpaceCommandHandler>();
        services.AddScoped<InviteMemberCommandHandler>();
        services.AddScoped<AcceptInvitationCommandHandler>();
        services.AddScoped<DeclineInvitationCommandHandler>();
        services.AddScoped<RevokeInvitationCommandHandler>();
        services.AddScoped<RemoveMemberCommandHandler>();
        services.AddScoped<LeaveWatchSpaceCommandHandler>();
        services.AddScoped<TransferOwnershipCommandHandler>();

        // Query handlers
        services.AddScoped<GetMyWatchSpacesQueryHandler>();
        services.AddScoped<GetWatchSpaceByIdQueryHandler>();
        services.AddScoped<ListInvitationsQueryHandler>();

        return services;
    }
}
