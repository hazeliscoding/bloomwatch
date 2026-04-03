using System.Text.RegularExpressions;
using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.Identity.Application.UseCases.ResetPassword;

/// <summary>
/// Validates a password-reset token and updates the user's password hash.
/// Token lookup, password update, and token invalidation are committed atomically.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository tokenRepository,
    IUserRepository userRepository,
    IRefreshTokenService tokenService,
    IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    // Min 8 chars, at least one uppercase, one lowercase, one digit
    private static readonly Regex PasswordStrengthRegex =
        new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", RegexOptions.Compiled);

    public async Task<ResetPasswordResult> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (!PasswordStrengthRegex.IsMatch(command.NewPassword))
            return ResetPasswordResult.WeakPassword;

        var tokenHash = tokenService.HashToken(command.Token);
        var resetToken = await tokenRepository.FindByHashAsync(tokenHash, cancellationToken);

        if (resetToken is null)
            return ResetPasswordResult.InvalidToken;

        if (resetToken.IsUsed)
            return ResetPasswordResult.TokenAlreadyUsed;

        if (resetToken.ExpiresAt <= DateTime.UtcNow)
            return ResetPasswordResult.TokenExpired;

        var user = await userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user is null)
            return ResetPasswordResult.InvalidToken;

        var newHash = passwordHasher.Hash(command.NewPassword);
        user.UpdatePasswordHash(newHash);
        resetToken.MarkUsed();

        await tokenRepository.SaveChangesAsync(cancellationToken);

        return ResetPasswordResult.Success;
    }
}
