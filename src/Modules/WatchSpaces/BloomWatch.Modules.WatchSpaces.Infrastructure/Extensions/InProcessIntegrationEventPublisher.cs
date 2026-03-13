using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Extensions;

/// <summary>
/// In-process integration event publisher. Future phases can replace this
/// with a message bus implementation without changing the Application layer.
/// </summary>
internal sealed class InProcessIntegrationEventPublisher(ILogger<InProcessIntegrationEventPublisher> logger)
    : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        logger.LogInformation(
            "Integration event published: {EventType} {@Event}",
            typeof(T).Name, integrationEvent);

        return Task.CompletedTask;
    }
}
