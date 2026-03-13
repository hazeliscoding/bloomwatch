using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Domain.Aggregates;

public sealed class User
{
    public UserId Id { get; private set; }
    public EmailAddress Email { get; private set; }
    public DisplayName DisplayName { get; private set; }
    public string PasswordHash { get; private set; }
    public AccountStatus AccountStatus { get; private set; }
    public bool IsEmailVerified { get; private set; }
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
}
