namespace BloomWatch.SharedKernel;

/// <summary>
/// Abstract base exception for all domain-rule violations across modules.
/// <para>
/// Each module defines its own subclass (e.g., <c>WatchSpaceDomainException</c>,
/// <c>AnimeTrackingDomainException</c>) so that the API layer can catch
/// <see cref="DomainException"/> uniformly while still distinguishing module-specific errors.
/// </para>
/// </summary>
/// <param name="message">A human-readable description of the domain rule that was violated.</param>
public abstract class DomainException(string message) : Exception(message);
