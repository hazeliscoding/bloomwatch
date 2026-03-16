using System.Text.RegularExpressions;

namespace BloomWatch.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a validated, normalized email address.
/// This value object enforces email format rules at construction time, ensuring that
/// every <see cref="EmailAddress"/> instance holds a syntactically valid, lowercase, trimmed address.
/// </summary>
/// <remarks>
/// <para>
/// <b>Validation rules:</b>
/// <list type="bullet">
///   <item>Must not be null, empty, or whitespace.</item>
///   <item>Must match the pattern <c>^[^@\s]+@[^@\s]+\.[^@\s]+$</c> (at least one character before @,
///         a domain with at least one dot, and no whitespace anywhere).</item>
/// </list>
/// </para>
/// <para>
/// <b>Normalization:</b> The address is trimmed and converted to lowercase invariant on creation,
/// so equality comparisons are case-insensitive by design.
/// </para>
/// </remarks>
public sealed class EmailAddress : IEquatable<EmailAddress>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Gets the normalized (lowercase, trimmed) email address string.
    /// </summary>
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="EmailAddress"/> from a raw string, applying validation and normalization.
    /// </summary>
    /// <param name="value">The raw email address string to validate.</param>
    /// <returns>A validated, normalized <see cref="EmailAddress"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, whitespace, or does not match
    /// the required email format.
    /// </exception>
    public static EmailAddress From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        return new EmailAddress(normalized);
    }

    /// <inheritdoc />
    public bool Equals(EmailAddress? other) => other is not null && Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EmailAddress e && Equals(e);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Returns the normalized email address string.
    /// </summary>
    /// <returns>The email address as a lowercase, trimmed string.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Determines whether two <see cref="EmailAddress"/> instances are equal by comparing their normalized values.
    /// </summary>
    public static bool operator ==(EmailAddress? left, EmailAddress? right)
        => left?.Value == right?.Value;

    /// <summary>
    /// Determines whether two <see cref="EmailAddress"/> instances are not equal.
    /// </summary>
    public static bool operator !=(EmailAddress? left, EmailAddress? right)
        => !(left == right);

    /// <summary>
    /// Implicitly converts an <see cref="EmailAddress"/> to its underlying <see cref="string"/> value.
    /// </summary>
    /// <param name="email">The <see cref="EmailAddress"/> to convert.</param>
    public static implicit operator string(EmailAddress email) => email.Value;
}
