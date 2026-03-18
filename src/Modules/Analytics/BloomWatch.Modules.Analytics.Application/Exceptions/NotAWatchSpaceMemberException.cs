namespace BloomWatch.Modules.Analytics.Application.Exceptions;

/// <summary>
/// Thrown when the requesting user is not a member of the target watch space.
/// </summary>
public sealed class NotAWatchSpaceMemberException()
    : Exception("You are not a member of this watch space.");
