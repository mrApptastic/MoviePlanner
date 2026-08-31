using MoviePlanner.Models;
using MoviePlanner.Services;

namespace MoviePlanner.Tests;

public class IcsGeneratorTests
{
    private readonly MovieEvent _testEvent = new()
    {
        Id = "test-event-123",
        Title = "Movie Night",
        DateTime = new DateTime(2026, 12, 25, 19, 0, 0, DateTimeKind.Utc),
        LocationId = "loc-1",
        MovieImdbId = "tt0111161",
        MovieTitle = "The Shawshank Redemption",
        MoviePoster = "https://example.com/poster.jpg"
    };

    private readonly Location _testLocation = new()
    {
        Id = "loc-1",
        Name = "Home Theater",
        Address = "123 Movie Lane"
    };

    private readonly Movie _testMovie = new()
    {
        ImdbId = "tt0111161",
        Title = "The Shawshank Redemption",
        Year = "1994",
        ImdbRating = "9.3",
        Genre = "Drama",
        Director = "Frank Darabont"
    };

    [Fact]
    public void GenerateEventIcs_ContainsRequiredFields()
    {
        var ics = IcsGenerator.GenerateEventIcs(_testEvent, _testLocation, _testMovie, "test@gmail.com");

        Assert.Contains("BEGIN:VCALENDAR", ics);
        Assert.Contains("END:VCALENDAR", ics);
        Assert.Contains("BEGIN:VEVENT", ics);
        Assert.Contains("END:VEVENT", ics);
        Assert.Contains("METHOD:REQUEST", ics);
        Assert.Contains("STATUS:CONFIRMED", ics);
        Assert.Contains("test-event-123@movieplanner", ics);
        Assert.Contains("Movie Night", ics);
        Assert.Contains("123 Movie Lane", ics);
        Assert.Contains("mailto:test@gmail.com", ics);
    }

    [Fact]
    public void GenerateEventIcs_ContainsMovieInfo()
    {
        var ics = IcsGenerator.GenerateEventIcs(_testEvent, _testLocation, _testMovie, "test@gmail.com");

        Assert.Contains("The Shawshank Redemption", ics);
        Assert.Contains("1994", ics);
        Assert.Contains("9.3", ics);
    }

    [Fact]
    public void GenerateEventIcs_HasCorrectDateFormat()
    {
        var ics = IcsGenerator.GenerateEventIcs(_testEvent, _testLocation, _testMovie, "test@gmail.com");

        Assert.Contains("DTSTART:20261225T190000Z", ics);
        // Event ends 3 hours later
        Assert.Contains("DTEND:20261225T220000Z", ics);
    }

    [Fact]
    public void GenerateCancellationIcs_ContainsRequiredFields()
    {
        var ics = IcsGenerator.GenerateCancellationIcs(_testEvent, "test@gmail.com");

        Assert.Contains("BEGIN:VCALENDAR", ics);
        Assert.Contains("METHOD:CANCEL", ics);
        Assert.Contains("STATUS:CANCELLED", ics);
        Assert.Contains("SEQUENCE:1", ics);
        Assert.Contains("test-event-123@movieplanner", ics);
        Assert.Contains("Cancelled", ics);
    }

    [Fact]
    public void GenerateEventIcs_SequenceIsZero()
    {
        var ics = IcsGenerator.GenerateEventIcs(_testEvent, _testLocation, _testMovie, "test@gmail.com");
        Assert.Contains("SEQUENCE:0", ics);
    }

    [Fact]
    public void GenerateCancellationIcs_SequenceIsOne()
    {
        var ics = IcsGenerator.GenerateCancellationIcs(_testEvent, "test@gmail.com");
        Assert.Contains("SEQUENCE:1", ics);
    }
}
