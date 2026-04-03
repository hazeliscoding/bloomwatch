using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.Identity.Application.UseCases.ForgotPassword;

/// <summary>
/// Handles the forgot-password flow: generates a single-use reset token, persists its hash,
/// and dispatches the reset email. Returns silently regardless of whether the email exists.
/// </summary>
public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IRefreshTokenService tokenService,
    IPasswordResetEmailSender emailSender) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        EmailAddress email;
        try
        {
            email = EmailAddress.From(command.Email);
        }
        catch
        {
            // Invalid email format — return silently (no enumeration)
            return;
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || user.AccountStatus != AccountStatus.Active)
            return;

        var plainToken = tokenService.GenerateToken();
        var tokenHash = tokenService.HashToken(plainToken);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var resetToken = PasswordResetToken.Create(user.Id, tokenHash, expiresAt);
        await tokenRepository.AddAsync(resetToken, cancellationToken);
        await tokenRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(user.Email.Value, plainToken, cancellationToken);
    }
}
