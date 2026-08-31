using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MoviePlanner.Models;

namespace MoviePlanner.Services;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly HttpClient _httpClient;
    private readonly IGoogleAuthService _authService;
    private string? _spreadsheetId;
    private const string SheetsApiBase = "https://sheets.googleapis.com/v4/spreadsheets";

    private static readonly string[] SheetNames = { "Settings", "FavoriteMovies", "Friends", "Groups", "Locations", "Events" };

    public GoogleSheetsService(HttpClient httpClient, IGoogleAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task<HttpClient> GetAuthorizedClientAsync()
    {
        var token = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _httpClient;
    }

    public async Task<string> InitializeSpreadsheetAsync()
    {
        var client = await GetAuthorizedClientAsync();

        // Try to find existing spreadsheet by checking settings
        if (!string.IsNullOrEmpty(_spreadsheetId))
            return _spreadsheetId;

        // Create new spreadsheet
        var createRequest = new
        {
            properties = new { title = "Movie Club Data" },
            sheets = SheetNames.Select(name => new
            {
                properties = new { title = name }
            }).ToArray()
        };

        var response = await client.PostAsJsonAsync(SheetsApiBase, createRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        _spreadsheetId = result.GetProperty("spreadsheetId").GetString()!;

        // Initialize headers for each sheet
        await SetSheetHeadersAsync(client);

        return _spreadsheetId;
    }

    private async Task SetSheetHeadersAsync(HttpClient client)
    {
        var headers = new Dictionary<string, List<string>>
        {
            ["Settings"] = new() { "Key", "Value" },
            ["FavoriteMovies"] = new() { "ImdbId", "Title", "Year", "Rated", "Released", "Runtime", "Genre", "Director", "Actors", "Plot", "Poster", "ImdbRating" },
            ["Friends"] = new() { "Id", "Name", "Email" },
            ["Groups"] = new() { "Id", "Name", "FriendIds" },
            ["Locations"] = new() { "Id", "Name", "Address" },
            ["Events"] = new() { "Id", "Title", "DateTime", "LocationId", "MovieImdbId", "MovieTitle", "MoviePoster", "InvitedFriendIds", "GoogleCalendarEventId", "IsCancelled" }
        };

        var data = new
        {
            valueInputOption = "RAW",
            data = headers.Select(h => new
            {
                range = $"{h.Key}!A1:{(char)('A' + h.Value.Count - 1)}1",
                values = new[] { h.Value.ToArray() }
            }).ToArray()
        };

        await client.PostAsJsonAsync($"{SheetsApiBase}/{_spreadsheetId}/values:batchUpdate", data);
    }

    public void SetSpreadsheetId(string id) => _spreadsheetId = id;

    private async Task<List<List<string>>> GetSheetDataAsync(string sheetName)
    {
        var client = await GetAuthorizedClientAsync();
        var response = await client.GetAsync($"{SheetsApiBase}/{_spreadsheetId}/values/{sheetName}");

        if (!response.IsSuccessStatusCode)
            return new List<List<string>>();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (!result.TryGetProperty("values", out var values))
            return new List<List<string>>();

        return values.EnumerateArray()
            .Skip(1) // Skip header row
            .Select(row => row.EnumerateArray().Select(cell => cell.GetString() ?? "").ToList())
            .ToList();
    }

    private async Task AppendRowAsync(string sheetName, List<string> row)
    {
        var client = await GetAuthorizedClientAsync();
        var data = new
        {
            values = new[] { row.ToArray() }
        };
        await client.PostAsJsonAsync(
            $"{SheetsApiBase}/{_spreadsheetId}/values/{sheetName}!A:Z:append?valueInputOption=RAW&insertDataOption=INSERT_ROWS",
            data);
    }

    private async Task UpdateSheetAsync(string sheetName, List<List<string>> rows)
    {
        var client = await GetAuthorizedClientAsync();

        // Clear existing data (keep headers)
        await client.PostAsJsonAsync(
            $"{SheetsApiBase}/{_spreadsheetId}/values/{sheetName}!A2:Z10000:clear",
            new { });

        if (rows.Count == 0) return;

        // Write all data
        var data = new
        {
            values = rows.Select(r => r.ToArray()).ToArray()
        };
        await client.PutAsJsonAsync(
            $"{SheetsApiBase}/{_spreadsheetId}/values/{sheetName}!A2?valueInputOption=RAW",
            data);
    }

    // Favorite Movies
    public async Task<List<Movie>> GetFavoriteMoviesAsync()
    {
        var rows = await GetSheetDataAsync("FavoriteMovies");
        return rows.Select(r => new Movie
        {
            ImdbId = r.ElementAtOrDefault(0) ?? "",
            Title = r.ElementAtOrDefault(1) ?? "",
            Year = r.ElementAtOrDefault(2) ?? "",
            Rated = r.ElementAtOrDefault(3) ?? "",
            Released = r.ElementAtOrDefault(4) ?? "",
            Runtime = r.ElementAtOrDefault(5) ?? "",
            Genre = r.ElementAtOrDefault(6) ?? "",
            Director = r.ElementAtOrDefault(7) ?? "",
            Actors = r.ElementAtOrDefault(8) ?? "",
            Plot = r.ElementAtOrDefault(9) ?? "",
            Poster = r.ElementAtOrDefault(10) ?? "",
            ImdbRating = r.ElementAtOrDefault(11) ?? ""
        }).ToList();
    }

    public async Task AddFavoriteMovieAsync(Movie movie)
    {
        await AppendRowAsync("FavoriteMovies", new List<string>
        {
            movie.ImdbId, movie.Title, movie.Year, movie.Rated, movie.Released,
            movie.Runtime, movie.Genre, movie.Director, movie.Actors, movie.Plot,
            movie.Poster, movie.ImdbRating
        });
    }

    public async Task RemoveFavoriteMovieAsync(string imdbId)
    {
        var movies = await GetFavoriteMoviesAsync();
        movies.RemoveAll(m => m.ImdbId == imdbId);
        var rows = movies.Select(m => new List<string>
        {
            m.ImdbId, m.Title, m.Year, m.Rated, m.Released, m.Runtime,
            m.Genre, m.Director, m.Actors, m.Plot, m.Poster, m.ImdbRating
        }).ToList();
        await UpdateSheetAsync("FavoriteMovies", rows);
    }

    // Friends
    public async Task<List<Friend>> GetFriendsAsync()
    {
        var rows = await GetSheetDataAsync("Friends");
        return rows.Select(r => new Friend
        {
            Id = r.ElementAtOrDefault(0) ?? "",
            Name = r.ElementAtOrDefault(1) ?? "",
            Email = r.ElementAtOrDefault(2) ?? ""
        }).ToList();
    }

    public async Task SaveFriendAsync(Friend friend)
    {
        var friends = await GetFriendsAsync();
        var existing = friends.FindIndex(f => f.Id == friend.Id);
        if (existing >= 0)
            friends[existing] = friend;
        else
            friends.Add(friend);

        var rows = friends.Select(f => new List<string> { f.Id, f.Name, f.Email }).ToList();
        await UpdateSheetAsync("Friends", rows);
    }

    public async Task DeleteFriendAsync(string friendId)
    {
        var friends = await GetFriendsAsync();
        friends.RemoveAll(f => f.Id == friendId);
        var rows = friends.Select(f => new List<string> { f.Id, f.Name, f.Email }).ToList();
        await UpdateSheetAsync("Friends", rows);
    }

    // Groups
    public async Task<List<Group>> GetGroupsAsync()
    {
        var rows = await GetSheetDataAsync("Groups");
        return rows.Select(r => new Group
        {
            Id = r.ElementAtOrDefault(0) ?? "",
            Name = r.ElementAtOrDefault(1) ?? "",
            FriendIds = (r.ElementAtOrDefault(2) ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        }).ToList();
    }

    public async Task SaveGroupAsync(Group group)
    {
        var groups = await GetGroupsAsync();
        var existing = groups.FindIndex(g => g.Id == group.Id);
        if (existing >= 0)
            groups[existing] = group;
        else
            groups.Add(group);

        var rows = groups.Select(g => new List<string> { g.Id, g.Name, string.Join(",", g.FriendIds) }).ToList();
        await UpdateSheetAsync("Groups", rows);
    }

    public async Task DeleteGroupAsync(string groupId)
    {
        var groups = await GetGroupsAsync();
        groups.RemoveAll(g => g.Id == groupId);
        var rows = groups.Select(g => new List<string> { g.Id, g.Name, string.Join(",", g.FriendIds) }).ToList();
        await UpdateSheetAsync("Groups", rows);
    }

    // Locations
    public async Task<List<Location>> GetLocationsAsync()
    {
        var rows = await GetSheetDataAsync("Locations");
        return rows.Select(r => new Location
        {
            Id = r.ElementAtOrDefault(0) ?? "",
            Name = r.ElementAtOrDefault(1) ?? "",
            Address = r.ElementAtOrDefault(2) ?? ""
        }).ToList();
    }

    public async Task SaveLocationAsync(Location location)
    {
        var locations = await GetLocationsAsync();
        var existing = locations.FindIndex(l => l.Id == location.Id);
        if (existing >= 0)
            locations[existing] = location;
        else
            locations.Add(location);

        var rows = locations.Select(l => new List<string> { l.Id, l.Name, l.Address }).ToList();
        await UpdateSheetAsync("Locations", rows);
    }

    public async Task DeleteLocationAsync(string locationId)
    {
        var locations = await GetLocationsAsync();
        locations.RemoveAll(l => l.Id == locationId);
        var rows = locations.Select(l => new List<string> { l.Id, l.Name, l.Address }).ToList();
        await UpdateSheetAsync("Locations", rows);
    }

    // Events
    public async Task<List<MovieEvent>> GetEventsAsync()
    {
        var rows = await GetSheetDataAsync("Events");
        return rows.Select(r => new MovieEvent
        {
            Id = r.ElementAtOrDefault(0) ?? "",
            Title = r.ElementAtOrDefault(1) ?? "",
            DateTime = DateTime.TryParse(r.ElementAtOrDefault(2), out var dt) ? dt : DateTime.Now,
            LocationId = r.ElementAtOrDefault(3) ?? "",
            MovieImdbId = r.ElementAtOrDefault(4) ?? "",
            MovieTitle = r.ElementAtOrDefault(5) ?? "",
            MoviePoster = r.ElementAtOrDefault(6) ?? "",
            InvitedFriendIds = (r.ElementAtOrDefault(7) ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            GoogleCalendarEventId = r.ElementAtOrDefault(8),
            IsCancelled = bool.TryParse(r.ElementAtOrDefault(9), out var c) && c
        }).ToList();
    }

    public async Task SaveEventAsync(MovieEvent movieEvent)
    {
        var events = await GetEventsAsync();
        var existing = events.FindIndex(e => e.Id == movieEvent.Id);
        if (existing >= 0)
            events[existing] = movieEvent;
        else
            events.Add(movieEvent);

        var rows = events.Select(EventToRow).ToList();
        await UpdateSheetAsync("Events", rows);
    }

    public async Task DeleteEventAsync(string eventId)
    {
        var events = await GetEventsAsync();
        events.RemoveAll(e => e.Id == eventId);
        var rows = events.Select(EventToRow).ToList();
        await UpdateSheetAsync("Events", rows);
    }

    private static List<string> EventToRow(MovieEvent e) => new()
    {
        e.Id, e.Title, e.DateTime.ToString("o"), e.LocationId,
        e.MovieImdbId, e.MovieTitle, e.MoviePoster,
        string.Join(",", e.InvitedFriendIds),
        e.GoogleCalendarEventId ?? "", e.IsCancelled.ToString()
    };

    // Settings
    public async Task<AppSettings> GetSettingsAsync()
    {
        var rows = await GetSheetDataAsync("Settings");
        var settings = new AppSettings();
        foreach (var row in rows)
        {
            var key = row.ElementAtOrDefault(0) ?? "";
            var value = row.ElementAtOrDefault(1) ?? "";
            switch (key)
            {
                case "OmdbApiKey": settings.OmdbApiKey = value; break;
                case "SpreadsheetId": settings.SpreadsheetId = value; break;
            }
        }
        return settings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var rows = new List<List<string>>
        {
            new() { "OmdbApiKey", settings.OmdbApiKey },
            new() { "SpreadsheetId", settings.SpreadsheetId }
        };
        await UpdateSheetAsync("Settings", rows);
    }
}
