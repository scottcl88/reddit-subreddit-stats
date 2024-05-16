using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Services;
using SubRedditStatsWeb.Controllers;

namespace Tests;

[TestClass]
public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly Mock<ISubRedditService> _subRedditServiceMock;
    private readonly Mock<IBaseRedditService> _baseRedditServiceMock;

    public HomeControllerTests()
    {
        _loggerMock = new Mock<ILogger<HomeController>>();
        _subRedditServiceMock = new Mock<ISubRedditService>();
        _baseRedditServiceMock = new Mock<IBaseRedditService>();
    }

    [TestMethod]
    public async Task GetSubredditStats_ReturnsJsonResult_WithSubredditStats()
    {
        // Arrange
        var subreddit = "example";
        var topPosts = new List<Post>();
        var topUsers = new List<UserPostCount>();
        var postFlairDistribution = new Dictionary<string, int>();
        var imageTextPostRatio = (1, 2);
        var rateLimit = new ThrottleStatus();

        _subRedditServiceMock.Setup(x => x.GetPostsByUpVotes(subreddit)).ReturnsAsync(topPosts);
        _subRedditServiceMock.Setup(x => x.GetTopUsers(subreddit)).ReturnsAsync(topUsers);
        _subRedditServiceMock.Setup(x => x.GetPostFlairDistribution(subreddit)).ReturnsAsync(postFlairDistribution);
        _subRedditServiceMock.Setup(x => x.GetImageTextPostRatio(subreddit)).ReturnsAsync(imageTextPostRatio);
        _baseRedditServiceMock.Setup(x => x.GetThrottleStatus()).Returns(rateLimit);

        var controller = new HomeController(_loggerMock.Object, _subRedditServiceMock.Object, _baseRedditServiceMock.Object);

        // Act
        var result = await controller.GetSubredditStats(subreddit);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        Assert.IsNotNull(result.Value);

        var json = JsonConvert.SerializeObject(result.Value);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        var rateLimitData = JsonConvert.DeserializeObject<ThrottleStatus>(data["RateLimit"].ToString());
        Assert.AreEqual(rateLimit.RateLimitUsed, rateLimitData.RateLimitUsed);
        Assert.AreEqual(rateLimit.RateLimitRemaining, rateLimitData.RateLimitRemaining);
        Assert.AreEqual(rateLimit.RateLimitReset, rateLimitData.RateLimitReset);
    }

    [TestMethod]
    public async Task GetSubredditStats_ReturnsJsonResult_WithError_WhenSubredditIsEmpty()
    {
        // Arrange
        var subreddit = "";
        var expectedError = "Subreddit cannot be empty";

        var controller = new HomeController(_loggerMock.Object, _subRedditServiceMock.Object, _baseRedditServiceMock.Object);

        // Act
        var result = await controller.GetSubredditStats(subreddit);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        Assert.IsNotNull(result.Value);

        var json = JsonConvert.SerializeObject(result.Value);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        Assert.AreEqual(expectedError, data["Error"]);
    }

    [TestMethod]
    public async Task GetSubredditStats_ReturnsJsonResult_WithError_WhenRedditRateLimitExceeded()
    {
        // Arrange
        var subreddit = "example";
        var expectedError = "Reddit rate limit exceeded";
        var rateLimit = new ThrottleStatus();

        _subRedditServiceMock.Setup(x => x.GetPostsByUpVotes(subreddit)).Throws(new RedditRateLimitException("exception"));
        _baseRedditServiceMock.Setup(x => x.GetThrottleStatus()).Returns(rateLimit);

        var controller = new HomeController(_loggerMock.Object, _subRedditServiceMock.Object, _baseRedditServiceMock.Object);

        // Act
        var result = await controller.GetSubredditStats(subreddit);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        Assert.IsNotNull(result.Value);

        var json = JsonConvert.SerializeObject(result.Value);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        Assert.AreEqual(expectedError, data["Error"]);

        var rateLimitData = JsonConvert.DeserializeObject<ThrottleStatus>(data["RateLimit"].ToString());
        Assert.AreEqual(rateLimit.RateLimitUsed, rateLimitData.RateLimitUsed);
        Assert.AreEqual(rateLimit.RateLimitRemaining, rateLimitData.RateLimitRemaining);
        Assert.AreEqual(rateLimit.RateLimitReset, rateLimitData.RateLimitReset);
    }

    [TestMethod]
    public async Task GetSubredditStats_ReturnsJsonResult_WithError_WhenErrorGettingSubredditStats()
    {
        // Arrange
        var subreddit = "example";
        var expectedError = "Error getting subreddit stats";

        _subRedditServiceMock.Setup(x => x.GetPostsByUpVotes(subreddit)).Throws(new Exception());

        var controller = new HomeController(_loggerMock.Object, _subRedditServiceMock.Object, _baseRedditServiceMock.Object);

        // Act
        var result = await controller.GetSubredditStats(subreddit);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        Assert.IsNotNull(result.Value);

        var json = JsonConvert.SerializeObject(result.Value);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        Assert.AreEqual(expectedError, data["Error"]);
    }
}