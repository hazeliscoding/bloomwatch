using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.Repositories;
using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Application.UseCases.Register;

public sealed class RegisterUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
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

public sealed class DuplicateEmailException(string email)
    : Exception($"An account with email '{email}' already exists.");

public sealed class RegistrationException(string message) : Exception(message);
