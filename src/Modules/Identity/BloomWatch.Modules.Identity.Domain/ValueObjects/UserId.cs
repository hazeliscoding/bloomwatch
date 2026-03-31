using BloomWatch.SharedKernel;

namespace BloomWatch.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a <see cref="BloomWatch.Modules.Identity.Domain.Aggregates.User"/> aggregate.
/// Wraps a <see cref="Guid"/> to prevent primitive obsession and accidental misuse of unrelated identifiers.
/// </summary>
/// <param name="Value">The underlying <see cref="Guid"/> that uniquely identifies a user.</param>
public readonly record struct UserId(Guid Value) : IEntityId<UserId>
{
    /// <summary>
    /// Generates a new, unique <see cref="UserId"/> backed by a fresh <see cref="Guid"/>.
    /// </summary>
    /// <returns>A new <see cref="UserId"/> instance with a randomly generated identifier.</returns>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a <see cref="UserId"/> from an existing <see cref="Guid"/> value.
    /// Use this when reconstituting an identifier from persistence or an external source.
    /// </summary>
    /// <param name="value">A <see cref="Guid"/> representing an existing user identifier.</param>
    /// <returns>A <see cref="UserId"/> wrapping the provided <paramref name="value"/>.</returns>
    public static UserId From(Guid value) => new(value);

    /// <summary>
    /// Returns the string representation of the underlying <see cref="Guid"/>.
    /// </summary>
    /// <returns>The GUID formatted as a lowercase hyphenated string (e.g., "550e8400-e29b-41d4-a716-446655440000").</returns>
    public override string ToString() => Value.ToString();
}
