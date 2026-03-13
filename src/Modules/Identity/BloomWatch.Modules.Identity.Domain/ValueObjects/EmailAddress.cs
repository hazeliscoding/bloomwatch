using System.Text.RegularExpressions;

namespace BloomWatch.Modules.Identity.Domain.ValueObjects;

public sealed class EmailAddress : IEquatable<EmailAddress>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        return new EmailAddress(normalized);
    }

    public bool Equals(EmailAddress? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is EmailAddress e && Equals(e);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(EmailAddress? left, EmailAddress? right)
        => left?.Value == right?.Value;

    public static bool operator !=(EmailAddress? left, EmailAddress? right)
        => !(left == right);

    public static implicit operator string(EmailAddress email) => email.Value;
}
