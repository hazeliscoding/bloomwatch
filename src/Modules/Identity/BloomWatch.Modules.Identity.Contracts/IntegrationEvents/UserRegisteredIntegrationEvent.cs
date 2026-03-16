namespace BloomWatch.Modules.Identity.Contracts.IntegrationEvents;

/// <summary>
/// Integration event raised when a new user account has been successfully registered
/// in the Identity module.
/// </summary>
/// <remarks>
/// Other modules (e.g., Notifications, Plant Management) can subscribe to this event
/// to perform side effects such as sending a welcome email or provisioning default
/// resources for the new user. This event is published after the user aggregate has
/// been persisted.
/// </remarks>
/// <param name="UserId">The unique identifier of the newly registered user.</param>
/// <param name="Email">The verified email address associated with the new account.</param>
/// <param name="DisplayName">The user-chosen display name at the time of registration.</param>
/// <param name="CreatedAtUtc">The UTC timestamp at which the account was created.</param>
public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime CreatedAtUtc);
