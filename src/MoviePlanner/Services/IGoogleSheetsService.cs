using MoviePlanner.Models;

namespace MoviePlanner.Services;

public interface IGoogleSheetsService
{
    Task<string> InitializeSpreadsheetAsync();
    Task<List<Movie>> GetFavoriteMoviesAsync();
    Task AddFavoriteMovieAsync(Movie movie);
    Task RemoveFavoriteMovieAsync(string imdbId);
    Task<List<Friend>> GetFriendsAsync();
    Task SaveFriendAsync(Friend friend);
    Task DeleteFriendAsync(string friendId);
    Task<List<Group>> GetGroupsAsync();
    Task SaveGroupAsync(Group group);
    Task DeleteGroupAsync(string groupId);
    Task<List<Location>> GetLocationsAsync();
    Task SaveLocationAsync(Location location);
    Task DeleteLocationAsync(string locationId);
    Task<List<MovieEvent>> GetEventsAsync();
    Task SaveEventAsync(MovieEvent movieEvent);
    Task DeleteEventAsync(string eventId);
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}
