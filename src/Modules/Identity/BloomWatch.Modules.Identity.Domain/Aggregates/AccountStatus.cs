namespace BloomWatch.Modules.Identity.Domain.Aggregates;

/// <summary>
/// Represents the lifecycle state of a user account within the Identity module.
/// </summary>
/// <remarks>
/// Account status governs whether a user can authenticate and interact with the system.
/// Transitions between statuses are enforced by the <see cref="User"/> aggregate root.
/// </remarks>
public enum AccountStatus
{
    /// <summary>
    /// The account is in good standing and the user can authenticate and use the system normally.
    /// This is the default status assigned when a new user registers.
    /// </summary>
    Active,

    /// <summary>
    /// The account has been suspended by an administrator or automated policy.
    /// A suspended user cannot authenticate until their account is reactivated.
    /// </summary>
    Suspended
}
