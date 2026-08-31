using MoviePlanner.Models;

namespace MoviePlanner.Tests;

public class ModelTests
{
    [Fact]
    public void Movie_DefaultValues_AreEmpty()
    {
        var movie = new Movie();
        Assert.Equal(string.Empty, movie.ImdbId);
        Assert.Equal(string.Empty, movie.Title);
        Assert.Equal(string.Empty, movie.Year);
        Assert.Equal(string.Empty, movie.Poster);
    }

    [Fact]
    public void Friend_HasDefaultId()
    {
        var friend = new Friend();
        Assert.False(string.IsNullOrEmpty(friend.Id));
        Assert.Equal(string.Empty, friend.Name);
        Assert.Equal(string.Empty, friend.Email);
    }

    [Fact]
    public void Friend_CanSetProperties()
    {
        var friend = new Friend
        {
            Name = "John Doe",
            Email = "john@example.com"
        };
        Assert.Equal("John Doe", friend.Name);
        Assert.Equal("john@example.com", friend.Email);
    }

    [Fact]
    public void Group_HasEmptyFriendIdsList()
    {
        var group = new Group();
        Assert.NotNull(group.FriendIds);
        Assert.Empty(group.FriendIds);
    }

    [Fact]
    public void Group_CanAddFriendIds()
    {
        var group = new Group { Name = "Movie Buffs" };
        group.FriendIds.Add("id1");
        group.FriendIds.Add("id2");
        Assert.Equal(2, group.FriendIds.Count);
    }

    [Fact]
    public void Location_HasDefaultId()
    {
        var location = new Location();
        Assert.False(string.IsNullOrEmpty(location.Id));
        Assert.Equal(string.Empty, location.Name);
        Assert.Equal(string.Empty, location.Address);
    }

    [Fact]
    public void MovieEvent_HasDefaultValues()
    {
        var evt = new MovieEvent();
        Assert.False(string.IsNullOrEmpty(evt.Id));
        Assert.Equal(string.Empty, evt.Title);
        Assert.NotNull(evt.InvitedFriendIds);
        Assert.Empty(evt.InvitedFriendIds);
        Assert.False(evt.IsCancelled);
        Assert.Null(evt.GoogleCalendarEventId);
    }

    [Fact]
    public void MovieEvent_DateTimeDefaultIsFuture()
    {
        var evt = new MovieEvent();
        Assert.True(evt.DateTime > DateTime.Now.AddDays(5));
    }

    [Fact]
    public void OmdbSearchResult_HasDefaults()
    {
        var result = new OmdbSearchResult();
        Assert.NotNull(result.Search);
        Assert.Empty(result.Search);
        Assert.Equal(string.Empty, result.Response);
    }

    [Fact]
    public void AppSettings_HasDefaults()
    {
        var settings = new AppSettings();
        Assert.Equal(string.Empty, settings.OmdbApiKey);
        Assert.Equal(string.Empty, settings.SpreadsheetId);
    }

    [Fact]
    public void GoogleTokenInfo_HasDefaults()
    {
        var token = new GoogleTokenInfo();
        Assert.Equal(string.Empty, token.AccessToken);
        Assert.Equal(string.Empty, token.Email);
        Assert.Equal(string.Empty, token.Name);
    }
}
