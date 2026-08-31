namespace MoviePlanner.Models;

public class Movie
{
    public string ImdbId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Rated { get; set; } = string.Empty;
    public string Released { get; set; } = string.Empty;
    public string Runtime { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Actors { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
    public string ImdbRating { get; set; } = string.Empty;
}

public class OmdbSearchResult
{
    public List<OmdbSearchItem> Search { get; set; } = new();
    public string TotalResults { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
}

public class OmdbSearchItem
{
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string imdbID { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
}
