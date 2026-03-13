using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Application.UseCases.GetProfile;

public sealed record GetUserProfileQuery(UserId UserId);

public sealed record UserProfileResult(
    Guid UserId,
    string Email,
    string DisplayName,
    string AccountStatus,
    bool IsEmailVerified,
    DateTime CreatedAtUtc);
