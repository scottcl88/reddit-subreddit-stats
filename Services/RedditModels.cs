namespace Services;

public class Post
{
    public string? Title { get; set; }
    public int UpVotes { get; set; }
}

public class UserPostCount
{
    public string? UserName { get; set; }
    public int PostCount { get; set; }
}

public class ThrottleStatus
{
    public string? RateLimitUsed { get; set; }
    public string? RateLimitRemaining { get; set; }
    public string? RateLimitReset { get; set; }
}