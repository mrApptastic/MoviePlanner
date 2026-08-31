namespace MoviePlanner.Models;

public class Group
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<string> FriendIds { get; set; } = new();
}
