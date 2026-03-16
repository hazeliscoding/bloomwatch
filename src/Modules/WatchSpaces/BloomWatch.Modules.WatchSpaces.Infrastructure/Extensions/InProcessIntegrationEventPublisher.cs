using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Extensions;

/// <summary>
/// In-process implementation of <see cref="IIntegrationEventPublisher"/> that logs
/// integration events instead of dispatching them to an external message bus.
/// </summary>
/// <remarks>
/// This is a placeholder for early development. Future phases can swap this with a
/// RabbitMQ, Kafka, or other message-bus-backed publisher without modifying the
/// Application layer, because consumers depend only on the
/// <see cref="IIntegrationEventPublisher"/> abstraction.
/// </remarks>
/// <param name="logger">Logger used to record published events at Information level.</param>
internal sealed class InProcessIntegrationEventPublisher(ILogger<InProcessIntegrationEventPublisher> logger)
    : IIntegrationEventPublisher
{
    /// <summary>
    /// Logs the integration event type and payload without dispatching it externally.
    /// </summary>
    /// <typeparam name="T">The integration event type (must be a reference type).</typeparam>
    /// <param name="integrationEvent">The event instance to publish.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation (unused in this implementation).</param>
    /// <returns>A completed task, since no external I/O is performed.</returns>
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        logger.LogInformation(
            "Integration event published: {EventType} {@Event}",
            typeof(T).Name, integrationEvent);

        return Task.CompletedTask;
    }
}
