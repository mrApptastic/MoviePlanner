using MoviePlanner.Models;

namespace MoviePlanner.Services;

public static class IcsGenerator
{
    public static string GenerateEventIcs(MovieEvent movieEvent, Location location, Movie movie, string organizer)
    {
        var dtStart = movieEvent.DateTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
        var dtEnd = movieEvent.DateTime.AddHours(3).ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
        var now = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

        return $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//MoviePlanner//EN
METHOD:REQUEST
BEGIN:VEVENT
UID:{movieEvent.Id}@movieplanner
DTSTART:{dtStart}
DTEND:{dtEnd}
DTSTAMP:{now}
ORGANIZER:mailto:{organizer}
SUMMARY:🎬 {movieEvent.Title}
DESCRIPTION:Movie: {movie.Title} ({movie.Year})\nRating: {movie.ImdbRating}\nGenre: {movie.Genre}
LOCATION:{location.Address}
STATUS:CONFIRMED
SEQUENCE:0
END:VEVENT
END:VCALENDAR";
    }

    public static string GenerateCancellationIcs(MovieEvent movieEvent, string organizer)
    {
        var dtStart = movieEvent.DateTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
        var dtEnd = movieEvent.DateTime.AddHours(3).ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
        var now = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

        return $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//MoviePlanner//EN
METHOD:CANCEL
BEGIN:VEVENT
UID:{movieEvent.Id}@movieplanner
DTSTART:{dtStart}
DTEND:{dtEnd}
DTSTAMP:{now}
ORGANIZER:mailto:{organizer}
SUMMARY:❌ Cancelled: {movieEvent.Title}
STATUS:CANCELLED
SEQUENCE:1
END:VEVENT
END:VCALENDAR";
    }
}
