using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Services;
using System.Net;

namespace Tests;

[TestClass]
public class BaseRedditServiceTests
{
    private readonly Mock<IRedditAuthService> _redditAuthServiceMock;
    private readonly Mock<ILogger<BaseRedditService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly AppSettings _appSettings;
    private readonly BaseRedditService _baseRedditService;

    public BaseRedditServiceTests()
    {
        _redditAuthServiceMock = new Mock<IRedditAuthService>();
        _loggerMock = new Mock<ILogger<BaseRedditService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _appSettings = new AppSettings
        {
            RedditBaseUrl = "https://www.reddit.com/",
            AccessToken = "your-access-token",
            UserAgent = "your-user-agent"
        };

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _baseRedditService = new BaseRedditService(_appSettings, _redditAuthServiceMock.Object, _loggerMock.Object, httpClient);
    }

    [TestMethod]
    public async Task GetTopPosts_ShouldReturnPosts()
    {
        // Arrange
        string subredditName = "test";
        string expectedUrl = $"{_appSettings.RedditBaseUrl}{subredditName}/top?limit=5";
        var expectedPosts = "{\"data\":{\"children\":[{}]}}";

        _redditAuthServiceMock.Setup(x => x.RefreshAccessTokenIfNeeded()).Returns(Task.CompletedTask);
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == expectedUrl), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedPosts.ToString())
            });

        // Act
        await _baseRedditService.GetTopPosts(subredditName);

        // Assert
        _redditAuthServiceMock.Verify(x => x.RefreshAccessTokenIfNeeded(), Times.Once);
        _httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == expectedUrl), ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetNewPosts_ShouldReturnPosts()
    {
        // Arrange
        string subredditName = "test";
        string expectedUrl = $"{_appSettings.RedditBaseUrl}{subredditName}/new?limit=100";
        var expectedPosts = "{\"data\":{\"children\":[{}]}}";

        _redditAuthServiceMock.Setup(x => x.RefreshAccessTokenIfNeeded()).Returns(Task.CompletedTask);
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == expectedUrl), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedPosts.ToString())
            });

        // Act
        await _baseRedditService.GetNewPosts(subredditName);

        // Assert
        _redditAuthServiceMock.Verify(x => x.RefreshAccessTokenIfNeeded(), Times.Once);
        _httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == expectedUrl), ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetNewPosts_ShouldUpdateThrottleStatus()
    {
        // Arrange
        var expectedThrottleStatus = new ThrottleStatus() { RateLimitRemaining = "1" };
        string subredditName = "test";
        string expectedUrl = $"{_appSettings.RedditBaseUrl}{subredditName}/new?limit=100";
        var expectedPosts = "{\"data\":{\"children\":[{}]}}";

        _redditAuthServiceMock.Setup(x => x.RefreshAccessTokenIfNeeded()).Returns(Task.CompletedTask);
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == expectedUrl), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedPosts.ToString()),
                Headers = { { "X-Ratelimit-Remaining", expectedThrottleStatus.RateLimitRemaining } }
            });

        // Act
        await _baseRedditService.GetNewPosts(subredditName);
        var result = _baseRedditService.GetThrottleStatus();

        // Assert
        Assert.AreEqual(expectedThrottleStatus.RateLimitRemaining, result.RateLimitRemaining);
    }
}