using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MoviePlanner.Models;

namespace MoviePlanner.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IGoogleAuthService _authService;
    private const string GmailApiBase = "https://gmail.googleapis.com/gmail/v1";

    public EmailService(HttpClient httpClient, IGoogleAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task SendEventInvitationAsync(MovieEvent movieEvent, Location location, Movie movie, Friend friend, string senderEmail)
    {
        var icsContent = GenerateIcsInvite(movieEvent, location, movie, senderEmail);
        var htmlBody = GenerateInviteHtml(movieEvent, location, movie);

        var boundary = $"boundary_{Guid.NewGuid():N}";
        var message = new StringBuilder();
        message.AppendLine($"From: {senderEmail}");
        message.AppendLine($"To: {friend.Email}");
        message.AppendLine($"Subject: 🎬 You're invited: {movieEvent.Title}");
        message.AppendLine("MIME-Version: 1.0");
        message.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
        message.AppendLine();
        message.AppendLine($"--{boundary}");
        message.AppendLine("Content-Type: text/html; charset=UTF-8");
        message.AppendLine();
        message.AppendLine(htmlBody);
        message.AppendLine();
        message.AppendLine($"--{boundary}");
        message.AppendLine("Content-Type: text/calendar; charset=UTF-8; method=REQUEST");
        message.AppendLine("Content-Disposition: attachment; filename=\"invite.ics\"");
        message.AppendLine();
        message.AppendLine(icsContent);
        message.AppendLine($"--{boundary}--");

        await SendRawEmailAsync(message.ToString());
    }

    public async Task SendCancellationAsync(MovieEvent movieEvent, Friend friend, string senderEmail)
    {
        var htmlBody = $@"
<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
<h2 style='color: #d32f2f;'>🎬 Event Cancelled</h2>
<p>Hi {friend.Name},</p>
<p>Unfortunately, the movie event <strong>{movieEvent.Title}</strong> scheduled for
<strong>{movieEvent.DateTime:dddd, MMMM d, yyyy 'at' h:mm tt}</strong> has been cancelled.</p>
<p>Movie: <strong>{movieEvent.MovieTitle}</strong></p>
{(string.IsNullOrEmpty(movieEvent.MoviePoster) || movieEvent.MoviePoster == "N/A" ? "" : $"<img src='{movieEvent.MoviePoster}' alt='{movieEvent.MovieTitle}' style='max-width: 200px; border-radius: 8px;' />")}
<p>We apologize for any inconvenience. Hope to see you at the next one!</p>
</body></html>";

        var icsContent = GenerateIcsCancellation(movieEvent, senderEmail);

        var boundary = $"boundary_{Guid.NewGuid():N}";
        var message = new StringBuilder();
        message.AppendLine($"From: {senderEmail}");
        message.AppendLine($"To: {friend.Email}");
        message.AppendLine($"Subject: ❌ Cancelled: {movieEvent.Title}");
        message.AppendLine("MIME-Version: 1.0");
        message.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
        message.AppendLine();
        message.AppendLine($"--{boundary}");
        message.AppendLine("Content-Type: text/html; charset=UTF-8");
        message.AppendLine();
        message.AppendLine(htmlBody);
        message.AppendLine();
        message.AppendLine($"--{boundary}");
        message.AppendLine("Content-Type: text/calendar; charset=UTF-8; method=CANCEL");
        message.AppendLine("Content-Disposition: attachment; filename=\"cancel.ics\"");
        message.AppendLine();
        message.AppendLine(icsContent);
        message.AppendLine($"--{boundary}--");

        await SendRawEmailAsync(message.ToString());
    }

    private async Task SendRawEmailAsync(string rawMessage)
    {
        var token = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawMessage))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var content = new StringContent(
            JsonSerializer.Serialize(new { raw = base64Message }),
            Encoding.UTF8,
            "application/json");

        await _httpClient.PostAsync($"{GmailApiBase}/users/me/messages/send", content);
    }

    private static string GenerateIcsInvite(MovieEvent movieEvent, Location location, Movie movie, string organizer)
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
DESCRIPTION:Movie: {movie.Title} ({movie.Year})\nRating: {movie.ImdbRating}\nGenre: {movie.Genre}\nDirector: {movie.Director}
LOCATION:{location.Address}
STATUS:CONFIRMED
SEQUENCE:0
END:VEVENT
END:VCALENDAR";
    }

    private static string GenerateIcsCancellation(MovieEvent movieEvent, string organizer)
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

    private static string GenerateInviteHtml(MovieEvent movieEvent, Location location, Movie movie)
    {
        return $@"
<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
<h2 style='color: #1976d2;'>🎬 You're Invited to a Movie Event!</h2>
<table style='width: 100%; border-collapse: collapse;'>
<tr>
<td style='vertical-align: top; padding: 10px;'>
{(string.IsNullOrEmpty(movie.Poster) || movie.Poster == "N/A" ? "" : $"<img src='{movie.Poster}' alt='{movie.Title}' style='max-width: 200px; border-radius: 8px;' />")}
</td>
<td style='vertical-align: top; padding: 10px;'>
<h3>{movieEvent.Title}</h3>
<p><strong>Movie:</strong> {movie.Title} ({movie.Year})</p>
<p><strong>Rating:</strong> ⭐ {movie.ImdbRating}/10</p>
<p><strong>Genre:</strong> {movie.Genre}</p>
<p><strong>Director:</strong> {movie.Director}</p>
<p><strong>Runtime:</strong> {movie.Runtime}</p>
<p><strong>Date:</strong> {movieEvent.DateTime:dddd, MMMM d, yyyy}</p>
<p><strong>Time:</strong> {movieEvent.DateTime:h:mm tt}</p>
<p><strong>Location:</strong> {location.Name}</p>
<p><strong>Address:</strong> {location.Address}</p>
</td>
</tr>
</table>
<p style='margin-top: 15px;'><strong>Plot:</strong> {movie.Plot}</p>
<p style='color: #666; margin-top: 20px;'>Please reply to let us know if you can make it! An .ics calendar file is attached for your convenience.</p>
</body></html>";
    }
}
