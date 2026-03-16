using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Application.UseCases.GetProfile;

/// <summary>
/// Query to retrieve a user's profile by their unique identifier.
/// </summary>
/// <param name="UserId">The strongly-typed unique identifier of the user whose profile to retrieve.</param>
public sealed record GetUserProfileQuery(UserId UserId);

/// <summary>
/// Represents the data returned when a user profile is successfully retrieved.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="AccountStatus">The current account status (for example, "Active" or "Suspended").</param>
/// <param name="IsEmailVerified">Whether the user has verified their email address.</param>
/// <param name="CreatedAtUtc">The UTC date and time when the user account was created.</param>
public sealed record UserProfileResult(
    Guid UserId,
    string Email,
    string DisplayName,
    string AccountStatus,
    bool IsEmailVerified,
    DateTime CreatedAtUtc);
