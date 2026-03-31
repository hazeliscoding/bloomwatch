using BloomWatch.SharedKernel;

namespace BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a <see cref="Aggregates.WatchSpace"/> aggregate.
/// Wraps a <see cref="Guid"/> to prevent accidental misuse of unrelated identifiers
/// and to make method signatures self-documenting.
/// </summary>
/// <param name="Value">The underlying <see cref="Guid"/> value of this identifier.</param>
public readonly record struct WatchSpaceId(Guid Value) : IEntityId<WatchSpaceId>
{
    /// <summary>
    /// Generates a new, unique <see cref="WatchSpaceId"/> backed by <see cref="Guid.NewGuid"/>.
    /// </summary>
    /// <returns>A freshly generated <see cref="WatchSpaceId"/>.</returns>
    public static WatchSpaceId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a <see cref="WatchSpaceId"/> from an existing <see cref="Guid"/> value.
    /// Use this when reconstituting an identifier from persistence or external input.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to wrap.</param>
    /// <returns>A <see cref="WatchSpaceId"/> wrapping the provided value.</returns>
    public static WatchSpaceId From(Guid value) => new(value);

    /// <summary>
    /// Returns the string representation of the underlying <see cref="Guid"/>.
    /// </summary>
    /// <returns>The GUID formatted as a lowercase hyphenated string (e.g., "d3b07384-d113-4ec8-9a5e-...").</returns>
    public override string ToString() => Value.ToString();
}
