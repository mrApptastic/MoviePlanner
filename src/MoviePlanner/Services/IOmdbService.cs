using MoviePlanner.Models;

namespace MoviePlanner.Services;

public interface IOmdbService
{
    Task<OmdbSearchResult> SearchMoviesAsync(string query, int page = 1);
    Task<Movie?> GetMovieByIdAsync(string imdbId);
}
