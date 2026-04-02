using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.Identity.Application.UseCases.RevokeToken;

public sealed class RevokeTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<RevokeTokenCommand>
{
    public async Task Handle(RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        var hash = refreshTokenService.HashToken(command.RefreshToken);
        var stored = await refreshTokenRepository.GetByHashAsync(hash, cancellationToken);

        // Idempotent: no error if not found or already revoked
        if (stored is null || stored.IsRevoked)
            return;

        await refreshTokenRepository.RevokeAsync(stored, cancellationToken);
    }
}
