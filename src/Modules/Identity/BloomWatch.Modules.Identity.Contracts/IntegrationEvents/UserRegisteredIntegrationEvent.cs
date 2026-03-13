namespace BloomWatch.Modules.Identity.Contracts.IntegrationEvents;

public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime CreatedAtUtc);
