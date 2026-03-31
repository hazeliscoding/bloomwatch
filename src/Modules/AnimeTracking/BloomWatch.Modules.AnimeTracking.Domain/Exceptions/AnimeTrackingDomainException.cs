using BloomWatch.SharedKernel;

namespace BloomWatch.Modules.AnimeTracking.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-rule violations within the AnimeTracking module.
/// </summary>
public class AnimeTrackingDomainException(string message) : DomainException(message);

/// <summary>
/// Thrown when attempting to add an anime that already exists in the watch space.
/// </summary>
public sealed class AnimeAlreadyInWatchSpaceException()
    : AnimeTrackingDomainException("This anime has already been added to the watch space.");

/// <summary>
/// Thrown when the requesting user is not a member of the target watch space.
/// </summary>
public sealed class NotAWatchSpaceMemberException()
    : AnimeTrackingDomainException("You are not a member of this watch space.");

/// <summary>
/// Thrown when an AniList media ID is not found in the local cache.
/// </summary>
public sealed class MediaNotFoundException(int aniListMediaId)
    : AnimeTrackingDomainException($"Media with AniList ID {aniListMediaId} was not found in the cache.");

/// <summary>
/// Thrown when shared state mutation violates domain constraints
/// (e.g. invalid episode count).
/// </summary>
public sealed class InvalidSharedStateException(string message)
    : AnimeTrackingDomainException(message);

/// <summary>
/// Thrown when participant progress mutation violates domain constraints
/// (e.g. invalid episode count).
/// </summary>
public sealed class InvalidParticipantProgressException(string message)
    : AnimeTrackingDomainException(message);

/// <summary>
/// Thrown when a participant rating mutation violates domain constraints
/// (e.g. score out of range or not in 0.5 increments).
/// </summary>
public sealed class InvalidRatingException(string message)
    : AnimeTrackingDomainException(message);
