using System.Net.Http.Json;
using System.Text.Json;
using MoviePlanner.Models;

namespace MoviePlanner.Services;

public class OmdbService : IOmdbService
{
    private readonly HttpClient _httpClient;
    private readonly IGoogleSheetsService _sheetsService;
    private const string OmdbBaseUrl = "https://www.omdbapi.com/";

    public OmdbService(HttpClient httpClient, IGoogleSheetsService sheetsService)
    {
        _httpClient = httpClient;
        _sheetsService = sheetsService;
    }

    private async Task<string> GetApiKeyAsync()
    {
        var settings = await _sheetsService.GetSettingsAsync();
        return settings.OmdbApiKey;
    }

    public async Task<OmdbSearchResult> SearchMoviesAsync(string query, int page = 1)
    {
        var apiKey = await GetApiKeyAsync();
        if (string.IsNullOrEmpty(apiKey))
            return new OmdbSearchResult { Response = "False" };

        var response = await _httpClient.GetFromJsonAsync<OmdbSearchResult>(
            $"{OmdbBaseUrl}?apikey={apiKey}&s={Uri.EscapeDataString(query)}&page={page}&type=movie");
        return response ?? new OmdbSearchResult { Response = "False" };
    }

    public async Task<Movie?> GetMovieByIdAsync(string imdbId)
    {
        var apiKey = await GetApiKeyAsync();
        if (string.IsNullOrEmpty(apiKey))
            return null;

        var json = await _httpClient.GetFromJsonAsync<JsonElement>(
            $"{OmdbBaseUrl}?apikey={apiKey}&i={imdbId}&plot=full");

        if (json.TryGetProperty("Response", out var resp) && resp.GetString() == "False")
            return null;

        return new Movie
        {
            ImdbId = json.GetProperty("imdbID").GetString() ?? "",
            Title = json.GetProperty("Title").GetString() ?? "",
            Year = json.GetProperty("Year").GetString() ?? "",
            Rated = json.TryGetProperty("Rated", out var rated) ? rated.GetString() ?? "" : "",
            Released = json.TryGetProperty("Released", out var released) ? released.GetString() ?? "" : "",
            Runtime = json.TryGetProperty("Runtime", out var runtime) ? runtime.GetString() ?? "" : "",
            Genre = json.TryGetProperty("Genre", out var genre) ? genre.GetString() ?? "" : "",
            Director = json.TryGetProperty("Director", out var director) ? director.GetString() ?? "" : "",
            Actors = json.TryGetProperty("Actors", out var actors) ? actors.GetString() ?? "" : "",
            Plot = json.TryGetProperty("Plot", out var plot) ? plot.GetString() ?? "" : "",
            Poster = json.TryGetProperty("Poster", out var poster) ? poster.GetString() ?? "" : "",
            ImdbRating = json.TryGetProperty("imdbRating", out var rating) ? rating.GetString() ?? "" : ""
        };
    }
}
