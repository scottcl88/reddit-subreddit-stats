namespace Services;

/// <summary>
/// Represents the interface for interacting with a subreddit.
/// </summary>
public interface ISubRedditService
{
    /// <summary>
    /// Retrieves a list of posts from a subreddit based on the number of upvotes.
    /// </summary>
    /// <param name="subredditName">The name of the subreddit.</param>
    /// <returns>A list of posts sorted by upvotes.</returns>
    Task<List<Post>> GetPostsByUpVotes(string subredditName);

    /// <summary>
    /// Retrieves the top users in a subreddit based on their post count.
    /// </summary>
    /// <param name="subredditName">The name of the subreddit.</param>
    /// <returns>A list of top users and their post counts.</returns>
    Task<List<UserPostCount>> GetTopUsers(string subredditName);

    /// <summary>
    /// Retrieves the distribution of post flairs in a subreddit.
    /// </summary>
    /// <param name="subredditName">The name of the subreddit.</param>
    /// <returns>A dictionary containing the post flair as the key and the count as the value.</returns>
    Task<Dictionary<string, int>> GetPostFlairDistribution(string subredditName);

    /// <summary>
    /// Retrieves the ratio of image posts to text posts in a subreddit.
    /// </summary>
    /// <param name="subredditName">The name of the subreddit.</param>
    /// <returns>A tuple containing the count of image posts and text posts.</returns>
    Task<(int ImagePosts, int TextPosts)> GetImageTextPostRatio(string subredditName);
}

public class SubRedditService : ISubRedditService
{
    private readonly IBaseRedditService _baseRedditService;

    public SubRedditService(IBaseRedditService baseRedditService)
    {
        _baseRedditService = baseRedditService;
    }

    /// <inheritdoc/>
    public async Task<List<Post>> GetPostsByUpVotes(string subredditName)
    {
        var posts = await _baseRedditService.GetTopPosts(subredditName);
        return posts.Select(p => new Post
        {
            Title = (string)p["data"]["title"],
            UpVotes = (int)p["data"]["ups"]
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<UserPostCount>> GetTopUsers(string subredditName)
    {
        var posts = await _baseRedditService.GetTopPosts(subredditName);
        var userPostCounts = posts.GroupBy(p => (string)p["data"]["author"])
                                  .Select(g => new UserPostCount { UserName = g.Key, PostCount = g.Count() })
                                  .OrderByDescending(up => up.PostCount)
                                  .Take(5)
                                  .ToList();
        return userPostCounts;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, int>> GetPostFlairDistribution(string subredditName)
    {
        var posts = await _baseRedditService.GetNewPosts(subredditName);
        var flairDistribution = posts.GroupBy(p => (string)p["data"]["link_flair_text"])
                                     .Where(g => !string.IsNullOrEmpty(g.Key))
                                     .ToDictionary(g => g.Key, g => g.Count());
        return flairDistribution;
    }

    /// <inheritdoc/>
    public async Task<(int ImagePosts, int TextPosts)> GetImageTextPostRatio(string subredditName)
    {
        var posts = await _baseRedditService.GetNewPosts(subredditName);
        int imagePosts = 0;
        int textPosts = 0;

        foreach (var post in posts)
        {
            var postHint = (string)post["data"]["post_hint"];
            if (postHint == "image")
            {
                imagePosts++;
            }
            else if (postHint == "self" || string.IsNullOrEmpty(postHint))
            {
                textPosts++;
            }
        }

        return (imagePosts, textPosts);
    }
}