using Microsoft.AspNetCore.Mvc;
using Services;
using SubRedditStatsWeb.Models;
using System.Diagnostics;

namespace SubRedditStatsWeb.Controllers;

/// <summary>
/// Represents the controller for the home page.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISubRedditService _subRedditService;
    private readonly IBaseRedditService _baseRedditService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="subRedditService">The subreddit service.</param>
    public HomeController(ILogger<HomeController> logger, ISubRedditService subRedditService, IBaseRedditService baseRedditService)
    {
        _logger = logger;
        _subRedditService = subRedditService;
        _baseRedditService = baseRedditService;
    }

    /// <summary>
    /// Displays the home page.
    /// </summary>
    /// <returns>The view result.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Gets the statistics for a subreddit.
    /// </summary>
    /// <param name="subreddit">The subreddit name.</param>
    /// <returns>The action result.</returns>
    [HttpGet]
    public async Task<JsonResult> GetSubredditStats(string subreddit)
    {
        subreddit = subreddit.Replace("/r/", "").Trim();
        if (string.IsNullOrEmpty(subreddit))
        {
            _logger.LogError("Subreddit cannot be empty");
            return Json(new { Error = "Subreddit cannot be empty" });
        }
        try
        {
            var topPosts = await _subRedditService.GetPostsByUpVotes(subreddit);
            var topUsers = await _subRedditService.GetTopUsers(subreddit);
            var postFlairDistribution = await _subRedditService.GetPostFlairDistribution(subreddit);
            var imageTextPostRatio = await _subRedditService.GetImageTextPostRatio(subreddit);
            var rateLimit = _baseRedditService.GetThrottleStatus();

            return Json(new
            {
                TopPosts = topPosts,
                TopUsers = topUsers,
                PostFlairDistribution = postFlairDistribution,
                ImageTextPostRatio = new { imageTextPostRatio.ImagePosts, imageTextPostRatio.TextPosts },
                RateLimit = new { rateLimit.RateLimitUsed, rateLimit.RateLimitRemaining, rateLimit.RateLimitReset }
            });
        }
        catch (RedditRateLimitException redditRateLimitException)
        {
            _logger.LogError(redditRateLimitException, "Reddit rate limit exceeded");
            var rateLimit = _baseRedditService.GetThrottleStatus();
            return Json(new
            {
                Error = "Reddit rate limit exceeded",
                RateLimit = new { rateLimit.RateLimitUsed, rateLimit.RateLimitRemaining, rateLimit.RateLimitReset }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subreddit stats");
            return Json(new
            {
                Error = "Error getting subreddit stats"
            });
        }
    }

    /// <summary>
    /// Displays the error page.
    /// </summary>
    /// <returns>The view result.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}