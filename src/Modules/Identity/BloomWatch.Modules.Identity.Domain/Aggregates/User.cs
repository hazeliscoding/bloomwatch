using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Domain.Aggregates;

/// <summary>
/// The aggregate root for user identity. Encapsulates a user's credentials, profile,
/// and account lifecycle within the Identity bounded context.
/// </summary>
/// <remarks>
/// <para>
/// <b>Invariants:</b>
/// <list type="bullet">
///   <item>A user always has a unique <see cref="UserId"/>, a valid <see cref="EmailAddress"/>,
///         a non-empty password hash, and a <see cref="DisplayName"/>.</item>
///   <item>Email verification starts as <c>false</c> on registration and must be confirmed
///         through a separate verification flow.</item>
///   <item>New users are created exclusively through the <see cref="Register"/> factory method,
///         which enforces all creation-time rules.</item>
/// </list>
/// </para>
/// <para>
/// <b>State transitions:</b> Account status begins as <see cref="AccountStatus.Active"/>
/// upon registration. Future operations (suspend, reactivate) will mutate the
/// <see cref="AccountStatus"/> property through dedicated methods on this aggregate.
/// </para>
/// </remarks>
public sealed class User
{
    /// <summary>
    /// Gets the unique identifier for this user.
    /// </summary>
    public UserId Id { get; private set; }

    /// <summary>
    /// Gets the user's email address. Normalized to lowercase and validated at construction time.
    /// </summary>
    public EmailAddress Email { get; private set; }

    /// <summary>
    /// Gets the user's display name shown in the UI and public-facing contexts.
    /// </summary>
    public DisplayName DisplayName { get; private set; }

    /// <summary>
    /// Gets the hashed representation of the user's password.
    /// The raw password is never stored; only the hash is persisted.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets the current lifecycle status of the user's account.
    /// </summary>
    /// <seealso cref="Aggregates.AccountStatus"/>
    public AccountStatus AccountStatus { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user has verified ownership of their email address.
    /// Starts as <c>false</c> at registration and becomes <c>true</c> after successful verification.
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this user account was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    // Required by EF Core
    private User() { }

    private User(
        UserId id,
        EmailAddress email,
        DisplayName displayName,
        string passwordHash,
        AccountStatus accountStatus,
        bool isEmailVerified,
        DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        AccountStatus = accountStatus;
        IsEmailVerified = isEmailVerified;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Registers a new user with the given email, password hash, and display name.
    /// The user receives a new unique identifier, is marked as <see cref="AccountStatus.Active"/>,
    /// and email verification is set to <c>false</c>.
    /// </summary>
    /// <param name="email">
    /// A validated <see cref="EmailAddress"/>. Callers are responsible for checking uniqueness
    /// via <see cref="Repositories.IUserRepository.ExistsWithEmailAsync"/> before calling this method.
    /// </param>
    /// <param name="passwordHash">
    /// The pre-hashed password. Must not be null, empty, or whitespace.
    /// Hashing is the caller's responsibility (e.g., the application service layer).
    /// </param>
    /// <param name="displayName">
    /// A validated <see cref="DisplayName"/> for the user's public profile.
    /// </param>
    /// <returns>A fully initialized <see cref="User"/> aggregate in the <see cref="AccountStatus.Active"/> state.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="passwordHash"/> is null, empty, or whitespace.
    /// </exception>
    public static User Register(EmailAddress email, string passwordHash, DisplayName displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        return new User(
            id: UserId.New(),
            email: email,
            displayName: displayName,
            passwordHash: passwordHash,
            accountStatus: AccountStatus.Active,
            isEmailVerified: false,
            createdAtUtc: DateTime.UtcNow);
    }

    /// <summary>
    /// Updates the user's password hash to the provided value.
    /// The caller is responsible for hashing the new password before invoking this method.
    /// </summary>
    /// <param name="newPasswordHash">The bcrypt hash of the new password.</param>
    public void UpdatePasswordHash(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }
}
