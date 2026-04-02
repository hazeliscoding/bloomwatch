using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.Identity.Application.UseCases.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var hash = refreshTokenService.HashToken(command.RefreshToken);
        var stored = await refreshTokenRepository.GetByHashAsync(hash, cancellationToken);

        if (stored is null || !stored.IsValid())
            throw new InvalidRefreshTokenException();

        var user = await userRepository.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
            throw new InvalidRefreshTokenException();

        // Revoke old token
        await refreshTokenRepository.RevokeAsync(stored, cancellationToken);

        // Issue new token pair
        var accessToken = jwtTokenGenerator.GenerateToken(user);

        var newPlainToken = refreshTokenService.GenerateToken();
        var newHash = refreshTokenService.HashToken(newPlainToken);
        var newExpiresAt = DateTime.UtcNow.AddDays(30);
        var newRefreshToken = Domain.Aggregates.RefreshToken.Create(stored.UserId, newHash, newExpiresAt);
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        return new RefreshTokenResult(accessToken.AccessToken, accessToken.ExpiresAt, newPlainToken, newExpiresAt);
    }
}

public sealed class InvalidRefreshTokenException()
    : Exception("Invalid or expired refresh token.");
