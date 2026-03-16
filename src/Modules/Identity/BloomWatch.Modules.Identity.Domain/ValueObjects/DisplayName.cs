namespace BloomWatch.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a user's display name, validated and normalized at construction time.
/// This value object ensures that every <see cref="DisplayName"/> instance holds a
/// non-empty, trimmed string within the allowed length.
/// </summary>
/// <remarks>
/// <para>
/// <b>Validation rules:</b>
/// <list type="bullet">
///   <item>Must not be null, empty, or whitespace-only.</item>
///   <item>Must not exceed <see cref="MaxLength"/> characters after trimming.</item>
/// </list>
/// </para>
/// <para>
/// <b>Normalization:</b> Leading and trailing whitespace is stripped on creation.
/// </para>
/// </remarks>
public sealed class DisplayName : IEquatable<DisplayName>
{
    /// <summary>
    /// The maximum number of characters allowed in a display name (100).
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Gets the validated, trimmed display name string.
    /// </summary>
    public string Value { get; }

    private DisplayName(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="DisplayName"/> from a raw string, applying validation and trimming.
    /// </summary>
    /// <param name="value">The raw display name string to validate.</param>
    /// <returns>A validated, trimmed <see cref="DisplayName"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, whitespace-only,
    /// or exceeds <see cref="MaxLength"/> characters after trimming.
    /// </exception>
    public static DisplayName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Display name cannot be empty or whitespace.", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Display name cannot exceed {MaxLength} characters.", nameof(value));

        return new DisplayName(trimmed);
    }

    /// <inheritdoc />
    public bool Equals(DisplayName? other) => other is not null && Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DisplayName d && Equals(d);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Returns the display name string.
    /// </summary>
    /// <returns>The trimmed display name value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="DisplayName"/> to its underlying <see cref="string"/> value.
    /// </summary>
    /// <param name="displayName">The <see cref="DisplayName"/> to convert.</param>
    public static implicit operator string(DisplayName displayName) => displayName.Value;
}
