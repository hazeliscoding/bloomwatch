namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;

public sealed class UpdateParticipantRatingRequest
{
    public decimal RatingScore { get; init; }

    private string? _ratingNotes;
    private bool _ratingNotesProvided;

    public string? RatingNotes
    {
        get => _ratingNotes;
        init
        {
            _ratingNotes = value;
            _ratingNotesProvided = true;
        }
    }

    public bool RatingNotesProvided => _ratingNotesProvided;
}
