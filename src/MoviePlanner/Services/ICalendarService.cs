using MoviePlanner.Models;

namespace MoviePlanner.Services;

public interface ICalendarService
{
    Task<string?> CreateCalendarEventAsync(MovieEvent movieEvent, Location location, List<Friend> friends);
    Task CancelCalendarEventAsync(string calendarEventId);
}
