using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Identity.Application.UseCases.ForgotPassword;

/// <summary>
/// Initiates the password-reset flow for the given email address.
/// Always succeeds silently, even if the email is not registered, to prevent user enumeration.
/// </summary>
/// <param name="Email">The email address of the account requesting a password reset.</param>
public sealed record ForgotPasswordCommand(string Email) : ICommand;
