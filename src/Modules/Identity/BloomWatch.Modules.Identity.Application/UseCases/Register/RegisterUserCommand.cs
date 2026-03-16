namespace BloomWatch.Modules.Identity.Application.UseCases.Register;

/// <summary>
/// Command to register a new user account in the system.
/// </summary>
/// <param name="Email">The email address for the new account. Must be a valid, unique email address.</param>
/// <param name="Password">The plain-text password for the new account. Must be at least 8 characters.</param>
/// <param name="DisplayName">The user-facing display name for the new account.</param>
public sealed record RegisterUserCommand(string Email, string Password, string DisplayName);

/// <summary>
/// Represents the successful result of a user registration operation.
/// </summary>
/// <param name="UserId">The unique identifier assigned to the newly created user.</param>
/// <param name="Email">The verified email address of the registered user.</param>
/// <param name="DisplayName">The display name of the registered user.</param>
public sealed record RegisterUserResult(Guid UserId, string Email, string DisplayName);
