using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Identity.Application.UseCases.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResult>;

public sealed record RefreshTokenResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
