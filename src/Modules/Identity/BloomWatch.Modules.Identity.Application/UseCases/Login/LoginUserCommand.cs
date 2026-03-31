using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Identity.Application.UseCases.Login;

/// <summary>
/// Command to authenticate a user with their email and password credentials.
/// </summary>
/// <param name="Email">The email address of the account to authenticate.</param>
/// <param name="Password">The plain-text password to verify against the stored hash.</param>
public sealed record LoginUserCommand(string Email, string Password) : ICommand<LoginUserResult>;

/// <summary>
/// Represents the successful result of a user login operation.
/// </summary>
/// <param name="AccessToken">The signed JWT access token for subsequent authenticated requests.</param>
/// <param name="ExpiresAt">The UTC date and time at which the access token expires.</param>
public sealed record LoginUserResult(string AccessToken, DateTime ExpiresAt);
