namespace BloomWatch.Modules.AnimeTracking.Domain.Enums;

/// <summary>
/// The tracking status for an anime within a watch space.
/// Used for both the shared group status and each participant's individual status.
/// </summary>
public enum AnimeStatus
{
    /// <summary>The anime is on the backlog and has not been started yet.</summary>
    Backlog,

    /// <summary>The anime is currently being watched.</summary>
    Watching,

    /// <summary>The anime has been completed.</summary>
    Finished,

    /// <summary>The anime is on hold and may be resumed later.</summary>
    Paused,

    /// <summary>The anime has been abandoned and will not be continued.</summary>
    Dropped
}
