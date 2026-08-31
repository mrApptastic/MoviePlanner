using MoviePlanner.Models;

namespace MoviePlanner.Services;

public interface IEmailService
{
    Task SendEventInvitationAsync(MovieEvent movieEvent, Location location, Movie movie, Friend friend, string senderEmail);
    Task SendCancellationAsync(MovieEvent movieEvent, Friend friend, string senderEmail);
}
