namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

/// <summary>
/// Publishes integration events to notify other modules or external systems
/// about state changes within the WatchSpaces module.
/// </summary>
/// <remarks>
/// Implementations should guarantee at-least-once delivery. Consumers must be idempotent.
/// </remarks>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event asynchronously.
    /// </summary>
    /// <typeparam name="T">The integration event type. Must be a reference type.</typeparam>
    /// <param name="integrationEvent">The event instance to publish.</param>
    /// <param name="cancellationToken">A token to cancel the publish operation.</param>
    /// <returns>A task that completes when the event has been accepted for delivery.</returns>
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class;
}
