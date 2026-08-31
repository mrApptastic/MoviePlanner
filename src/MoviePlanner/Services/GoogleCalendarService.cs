using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MoviePlanner.Models;

namespace MoviePlanner.Services;

public class GoogleCalendarService : ICalendarService
{
    private readonly HttpClient _httpClient;
    private readonly IGoogleAuthService _authService;
    private const string CalendarApiBase = "https://www.googleapis.com/calendar/v3";

    public GoogleCalendarService(HttpClient httpClient, IGoogleAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<string?> CreateCalendarEventAsync(MovieEvent movieEvent, Location location, List<Friend> friends)
    {
        var token = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var calendarEvent = new
        {
            summary = $"🎬 {movieEvent.Title}",
            description = $"Movie: {movieEvent.MovieTitle}\nLocation: {location.Name}\nAddress: {location.Address}",
            location = location.Address,
            start = new
            {
                dateTime = movieEvent.DateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                timeZone = "UTC"
            },
            end = new
            {
                dateTime = movieEvent.DateTime.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss"),
                timeZone = "UTC"
            },
            attendees = friends.Select(f => new
            {
                email = f.Email,
                responseStatus = "needsAction"
            }).ToArray(),
            reminders = new
            {
                useDefault = false,
                overrides = new[]
                {
                    new { method = "email", minutes = 1440 },
                    new { method = "popup", minutes = 60 }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{CalendarApiBase}/calendars/primary/events?sendUpdates=all",
            calendarEvent);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id").GetString();
        }

        return null;
    }

    public async Task CancelCalendarEventAsync(string calendarEventId)
    {
        var token = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _httpClient.DeleteAsync(
            $"{CalendarApiBase}/calendars/primary/events/{calendarEventId}?sendUpdates=all");
    }
}
