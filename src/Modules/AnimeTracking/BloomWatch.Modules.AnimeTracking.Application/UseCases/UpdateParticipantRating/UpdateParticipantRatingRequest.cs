namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;

/// <summary>
/// Request body for submitting or updating the calling user's personal rating for a tracked anime.
/// </summary>
/// <remarks>
/// <para>The <see cref="RatingNotes"/> property uses a custom setter to distinguish between
/// "not provided" (<c>RatingNotes</c> absent from the JSON payload) and "explicitly set to null"
/// (user wants to clear their notes). Check <see cref="RatingNotesProvided"/> to differentiate.</para>
/// </remarks>
public sealed class UpdateParticipantRatingRequest
{
    /// <summary>
    /// The user's numeric rating score for the anime (e.g., 1.0–10.0).
    /// </summary>
    public decimal RatingScore { get; init; }

    private string? _ratingNotes;
    private bool _ratingNotesProvided;

    /// <summary>
    /// Optional free-text notes accompanying the rating, or <c>null</c> to clear existing notes.
    /// When this property is absent from the JSON payload, existing notes are left unchanged.
    /// </summary>
    public string? RatingNotes
    {
        get => _ratingNotes;
        init
        {
            _ratingNotes = value;
            _ratingNotesProvided = true;
        }
    }

    /// <summary>
    /// Indicates whether <see cref="RatingNotes"/> was explicitly included in the request payload.
    /// When <c>false</c>, the notes field should not be modified.
    /// </summary>
    public bool RatingNotesProvided => _ratingNotesProvided;
}
