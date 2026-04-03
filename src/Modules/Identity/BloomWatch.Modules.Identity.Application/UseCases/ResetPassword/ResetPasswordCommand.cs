using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Identity.Application.UseCases.ResetPassword;

/// <summary>
/// Resets a user's password using a valid, single-use password-reset token.
/// </summary>
/// <param name="Token">The plain (un-hashed) reset token from the email link.</param>
/// <param name="NewPassword">The new password to set for the account.</param>
public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand<ResetPasswordResult>;

/// <summary>Outcome of a <see cref="ResetPasswordCommand"/>.</summary>
public enum ResetPasswordResult
{
    Success,
    InvalidToken,
    TokenExpired,
    TokenAlreadyUsed,
    WeakPassword,
}
