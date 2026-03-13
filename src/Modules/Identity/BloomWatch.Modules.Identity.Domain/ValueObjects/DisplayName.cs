namespace BloomWatch.Modules.Identity.Domain.ValueObjects;

public sealed class DisplayName : IEquatable<DisplayName>
{
    public const int MaxLength = 100;

    public string Value { get; }

    private DisplayName(string value) => Value = value;

    public static DisplayName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Display name cannot be empty or whitespace.", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Display name cannot exceed {MaxLength} characters.", nameof(value));

        return new DisplayName(trimmed);
    }

    public bool Equals(DisplayName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is DisplayName d && Equals(d);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(DisplayName displayName) => displayName.Value;
}
