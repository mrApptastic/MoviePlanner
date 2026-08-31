# 🎬 Movie Club

A databaseless Blazor WebAssembly PWA for planning movie nights with friends. Your data lives entirely in your own Google Sheets — no server-side database needed.

## Features

- **Google Authentication** — Sign in with your Google account to access all features
- **Google Sheets as Database** — All your data (movies, friends, groups, locations, events) is stored in a Google Sheets spreadsheet in your own Google Drive
- **Movie Search** — Search the [OMDb API](https://www.omdbapi.com/) for movies and add them to your favorites
- **Friends & Groups** — Manage friends (name + email) and organize them into groups
- **Locations** — Add and manage locations with addresses for your movie events
- **Event Scheduling** — Schedule movie events with a date/time, location, movie, and invited friends
  - Creates events in your **Google Calendar**
  - Gmail friends are invited via Google Calendar
  - Non-Gmail friends receive an email with an **.ics calendar attachment**
  - HTML-formatted emails with movie poster, plot, and event details
- **Event Cancellation** — Cancel events, which removes them from your calendar and sends cancellation notices (with .ics CANCEL method) to all invitees
- **PWA with Offline Support** — Installable as a Progressive Web App with service worker caching
- **Automatic Updates** — Service worker handles update detection with a user-friendly "Update Now" prompt

## Prerequisites

1. **Google Cloud Project** — Create a project at [Google Cloud Console](https://console.cloud.google.com/)
   - Enable these APIs:
     - Google Sheets API
     - Google Calendar API
     - Gmail API
   - Create an OAuth 2.0 Client ID (Web application type)
   - Add your deployment URL to Authorized JavaScript origins
   - Add your deployment URL to Authorized redirect URIs

2. **OMDb API Key** — Get a free key at [omdbapi.com/apikey.aspx](https://www.omdbapi.com/apikey.aspx)

## Getting Started

### Local Development

```bash
# Clone the repository
git clone https://github.com/mrApptastic/MoviePlanner.git
cd MoviePlanner

# Run the application
dotnet run --project src/MoviePlanner

# Run tests
dotnet test tests/MoviePlanner.Tests
```

### First-Time Setup

1. Open the app and sign in with your Google account
2. You'll be prompted for your Google OAuth Client ID
3. Go to **Settings** and click "Create Movie Club Spreadsheet"
4. Enter your OMDb API key and save

### Deployment

The project includes a GitHub Actions workflow that automatically deploys to GitHub Pages on pushes to `main`.

To enable deployment:
1. Go to repository **Settings** → **Pages**
2. Set Source to **GitHub Actions**
3. Push to `main` to trigger the deployment

The app will be available at `https://<username>.github.io/MoviePlanner/`

## Project Structure

```
MoviePlanner/
├── .github/workflows/
│   └── deploy.yml              # GitHub Actions CI/CD pipeline
├── src/MoviePlanner/
│   ├── Models/                 # Data models
│   │   ├── Movie.cs            # Movie and OMDb search models
│   │   ├── Friend.cs           # Friend model
│   │   ├── Group.cs            # Friend group model
│   │   ├── Location.cs         # Location model
│   │   ├── MovieEvent.cs       # Scheduled event model
│   │   ├── AppSettings.cs      # Application settings
│   │   └── GoogleTokenInfo.cs  # Auth token model
│   ├── Services/               # Business logic
│   │   ├── GoogleAuthService.cs       # Google OAuth authentication
│   │   ├── GoogleSheetsService.cs     # CRUD operations via Sheets API
│   │   ├── OmdbService.cs            # OMDb movie search
│   │   ├── GoogleCalendarService.cs   # Google Calendar integration
│   │   ├── EmailService.cs           # Gmail API email sending
│   │   └── IcsGeneratorService.cs    # ICS calendar file generation
│   ├── Pages/                  # Blazor pages
│   │   ├── Home.razor          # Dashboard
│   │   ├── Movies.razor        # Movie search & favorites
│   │   ├── Friends.razor       # Friends & groups management
│   │   ├── Locations.razor     # Location management
│   │   ├── Events.razor        # Event scheduling & cancellation
│   │   └── Settings.razor      # App configuration
│   ├── Layout/                 # Layout components
│   ├── wwwroot/                # Static assets
│   │   ├── js/auth.js          # Google Identity Services interop
│   │   ├── service-worker.js   # Dev service worker
│   │   └── service-worker.published.js  # Production service worker with update handling
│   └── Program.cs              # App entry point & DI setup
└── tests/MoviePlanner.Tests/   # Unit tests
    ├── ModelTests.cs           # Model validation tests
    └── IcsGeneratorTests.cs    # ICS file generation tests
```

## Architecture

### Data Storage

All data is stored in a single Google Sheets spreadsheet with the following sheets:
- **Settings** — API keys and configuration
- **FavoriteMovies** — Saved movie details from OMDb
- **Friends** — Friend names and email addresses
- **Groups** — Named groups with friend ID references
- **Locations** — Event locations with addresses
- **Events** — Scheduled movie events with all details

### APIs Used

| API | Purpose |
|-----|---------|
| Google Identity Services | OAuth 2.0 authentication |
| Google Sheets API | Data storage (CRUD) |
| Google Calendar API | Event creation & cancellation |
| Gmail API | Sending email invitations |
| OMDb API | Movie search and details |

### Service Worker Updates

The app uses a service worker for offline caching. When a new version is deployed:
1. The service worker detects the update in the background
2. A banner appears: "A new version is available"
3. The user clicks "Update Now"
4. The service worker activates and the page reloads

## Technology Stack

- **Framework**: .NET 10 / Blazor WebAssembly
- **UI**: Bootstrap 5
- **Hosting**: GitHub Pages (static)
- **CI/CD**: GitHub Actions
- **Authentication**: Google OAuth 2.0 (via Google Identity Services)
- **Data**: Google Sheets API v4
- **Testing**: xUnit, bUnit, Moq

## License

This project is provided as-is for educational and personal use.
