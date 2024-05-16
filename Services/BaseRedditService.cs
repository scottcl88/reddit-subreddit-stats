using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Services;

public interface IBaseRedditService
{
    Task<JArray> GetTopPosts(string subredditName);

    Task<JArray> GetNewPosts(string subredditName);

    ThrottleStatus GetThrottleStatus();
}

public class BaseRedditService : IBaseRedditService
{
    private readonly AppSettings _appSettings;
    private readonly IRedditAuthService _redditAuthService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BaseRedditService> _logger;

    private DateTime lastRequestTime = DateTime.MinValue;
    private int requestCount = 0;
    private readonly object lockObject = new object();

    protected ThrottleStatus ThrottleStatus { get; set; } = new ThrottleStatus();

    public BaseRedditService(AppSettings appSettings, IRedditAuthService redditAuthService, ILogger<BaseRedditService> logger, HttpClient httpClient)
    {
        _appSettings = appSettings;
        _redditAuthService = redditAuthService;
        _logger = logger;
        _httpClient = httpClient;
    }

    public ThrottleStatus GetThrottleStatus()
    {
        return ThrottleStatus;
    }

    public async Task<JArray> GetTopPosts(string subredditName)
    {
        await _redditAuthService.RefreshAccessTokenIfNeeded();
        var posts = await GetRedditData($"{_appSettings.RedditBaseUrl}{subredditName}/top?limit=5");
        return posts;
    }

    public async Task<JArray> GetNewPosts(string subredditName)
    {
        await _redditAuthService.RefreshAccessTokenIfNeeded();
        var posts = await GetRedditData($"{_appSettings.RedditBaseUrl}{subredditName}/new?limit=100");
        return posts;
    }

    private void UpdateRateLimits(HttpResponseMessage? response)
    {
        // Rate limiting
        string? rateLimitUsed = string.Empty;
        string? rateLimitRemaining = string.Empty;
        string? rateLimitReset = string.Empty;

        IEnumerable<string>? rateLimitUsedValues = null;
        response?.Headers.TryGetValues("X-Ratelimit-Used", out rateLimitUsedValues);
        if (rateLimitUsedValues != null && rateLimitUsedValues.Any())
        {
            rateLimitUsed = rateLimitUsedValues.First();
        }

        IEnumerable<string>? rateLimitRemainingValues = null;
        response?.Headers.TryGetValues("X-Ratelimit-Remaining", out rateLimitRemainingValues);
        if (rateLimitRemainingValues != null && rateLimitRemainingValues.Any())
        {
            rateLimitRemaining = rateLimitRemainingValues.First();
        }

        IEnumerable<string>? rateLimitResetValues = null;
        response?.Headers.TryGetValues("X-Ratelimit-Reset", out rateLimitResetValues);
        if (rateLimitResetValues != null && rateLimitResetValues.Any())
        {
            rateLimitReset = rateLimitResetValues.First();
        }

        ThrottleStatus.RateLimitReset = rateLimitReset;
        ThrottleStatus.RateLimitUsed = rateLimitUsed;
        ThrottleStatus.RateLimitRemaining = rateLimitRemaining;

        _logger.LogInformation($"Rate Limit Used: {rateLimitUsed}, Remaining: {rateLimitRemaining}, Reset in: {rateLimitReset} seconds");
    }

    private void CheckRateLimit()
    {
        lock (lockObject)
        {
            TimeSpan timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
            if (timeSinceLastRequest < TimeSpan.FromSeconds(60))
            {
                requestCount++;
                if (requestCount >= 100)
                {
                    double waitTime = (60 - timeSinceLastRequest.TotalSeconds) + 1;
                    throw new RedditRateLimitException($"Rate limit exceeded. Please wait {waitTime} seconds before making another request.");
                }
            }
            else
            {
                lastRequestTime = DateTime.UtcNow;
                requestCount = 1;
            }
        }
    }

    private async Task<JArray> GetRedditData(string url)
    {
        int maxRetries = 5;
        int delay = 1000; // initial delay in milliseconds

        CheckRateLimit();

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.AccessToken);
                request.Headers.TryAddWithoutValidation("User-Agent", _appSettings.UserAgent);

                var response = await _httpClient.SendAsync(request);

                UpdateRateLimits(response);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JObject.Parse(responseContent);
                    return jsonResponse.data.children;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogInformation("Rate limited by Reddit API. Retrying after delay.");

                    var retryAfter = response.Headers.RetryAfter;
                    if (retryAfter != null)
                    {
                        await Task.Delay(retryAfter.Delta ?? TimeSpan.FromSeconds(60));
                    }
                    else
                    {
                        await Task.Delay(delay);
                        delay *= 2; // Exponential backoff
                    }

                    // If it's the last retry and still getting a rate limit error, throw RedditRateLimitException
                    if (retry == maxRetries - 1)
                    {
                        throw new RedditRateLimitException("Rate limit exceeded after multiple retries.");
                    }
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data from Reddit");

                // If it's the last retry and still encountering an error, throw RedditException
                if (retry == maxRetries - 1)
                {
                    throw new RedditException("Failed to get data from Reddit after multiple retries.");
                }
            }
        }

        // This line will only be reached if all retries fail
        throw new RedditException("Failed to get data from Reddit after multiple retries.");
    }
}