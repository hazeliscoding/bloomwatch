namespace BloomWatch.Modules.WatchSpaces.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-rule violations within the WatchSpaces module.
/// Thrown when an operation would violate an invariant of the
/// <see cref="Aggregates.WatchSpace"/> aggregate or its child entities.
/// </summary>
/// <param name="message">A human-readable description of the domain rule that was violated.</param>
public class WatchSpaceDomainException(string message) : Exception(message);
