namespace MoviePlanner.Models;

public class MovieEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.Now.AddDays(7);
    public string LocationId { get; set; } = string.Empty;
    public string MovieImdbId { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePoster { get; set; } = string.Empty;
    public List<string> InvitedFriendIds { get; set; } = new();
    public string? GoogleCalendarEventId { get; set; }
    public bool IsCancelled { get; set; }
}
