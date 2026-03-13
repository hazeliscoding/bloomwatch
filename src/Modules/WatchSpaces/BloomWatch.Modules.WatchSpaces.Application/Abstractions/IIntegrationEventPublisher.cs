namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class;
}
