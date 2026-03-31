namespace BloomWatch.SharedKernel;

/// <summary>
/// Contract for strongly-typed GUID identifiers used as entity or aggregate IDs.
/// <para>
/// Each module defines its own concrete <see langword="readonly record struct"/>
/// (e.g., <c>UserId</c>, <c>WatchSpaceId</c>) implementing this interface.
/// The interface enables generic infrastructure code (e.g., EF Core value converters)
/// while preserving compile-time type safety across bounded contexts.
/// </para>
/// </summary>
/// <typeparam name="TSelf">
/// The concrete ID type implementing this interface (CRTP / self-referencing generic).
/// </typeparam>
public interface IEntityId<TSelf> where TSelf : struct, IEntityId<TSelf>
{
    /// <summary>
    /// Gets the underlying <see cref="Guid"/> value.
    /// </summary>
    Guid Value { get; }

    /// <summary>
    /// Generates a new, unique identifier backed by <see cref="Guid.NewGuid"/>.
    /// </summary>
    static abstract TSelf New();

    /// <summary>
    /// Reconstitutes an identifier from an existing <see cref="Guid"/>
    /// (e.g., when reading from persistence or an external source).
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to wrap.</param>
    static abstract TSelf From(Guid value);
}
