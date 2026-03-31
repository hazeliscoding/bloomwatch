using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.Identity.Application.UseCases.Register;

/// <summary>
/// Handles user registration by validating input, ensuring email uniqueness,
/// hashing the password, and persisting the new user aggregate.
/// </summary>
public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Processes a user registration command by creating a new user account.
    /// </summary>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    ///   <item>Validates the email address and display name as domain value objects.</item>
    ///   <item>Validates that the password is at least 8 characters long.</item>
    ///   <item>Checks that no existing account uses the same email address.</item>
    ///   <item>Hashes the plain-text password.</item>
    ///   <item>Creates the <see cref="User"/> aggregate via the domain factory method.</item>
    ///   <item>Persists the new user to the repository.</item>
    /// </list>
    /// </remarks>
    /// <param name="command">The registration command containing email, password, and display name.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="RegisterUserResult"/> containing the new user's ID, email, and display name.</returns>
    /// <exception cref="RegistrationException">Thrown when the password is null or fewer than 8 characters.</exception>
    /// <exception cref="DuplicateEmailException">Thrown when an account with the given email already exists.</exception>
    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = EmailAddress.From(command.Email);
        var displayName = DisplayName.From(command.DisplayName);

        if (command.Password is null || command.Password.Length < 8)
            throw new RegistrationException("Password must be at least 8 characters.");

        var emailTaken = await _userRepository.ExistsWithEmailAsync(email, cancellationToken);
        if (emailTaken)
            throw new DuplicateEmailException(email.Value);

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Register(email, passwordHash, displayName);

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserResult(user.Id.Value, user.Email.Value, user.DisplayName.Value);
    }
}

/// <summary>
/// Thrown when a user attempts to register with an email address that is already in use.
/// </summary>
/// <param name="email">The duplicate email address.</param>
public sealed class DuplicateEmailException(string email)
    : Exception($"An account with email '{email}' already exists.");

/// <summary>
/// Thrown when a registration attempt fails due to invalid input or a business rule violation.
/// </summary>
/// <param name="message">A message describing the registration failure.</param>
public sealed class RegistrationException(string message) : Exception(message);
