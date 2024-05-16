namespace Services;

public class RedditRateLimitException : Exception
{
    public RedditRateLimitException(string message) : base(message)
    {
    }
}

public class RedditException : Exception
{
    public RedditException(string message) : base(message)
    {
    }
}