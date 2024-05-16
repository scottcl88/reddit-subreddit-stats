using Moq;
using Moq.Protected;
using Services;
using System.Net;
using System.Net.Http.Headers;

namespace Tests;

[TestClass]
public class RedditAuthServiceTests
{
    private Mock<AppSettings> _mockAppSettings;
    private RedditAuthService _redditAuthService;

    public RedditAuthServiceTests()
    {
        _mockAppSettings = new Mock<AppSettings>();
        _redditAuthService = new RedditAuthService(_mockAppSettings.Object, new HttpClient());
    }

    [TestMethod]
    public async Task RefreshAccessTokenIfNeeded_WithValidRefreshToken_ShouldRefreshAccessToken()
    {
        // Arrange
        _mockAppSettings.Setup(x => x.RefreshToken).Returns("valid_refresh_token");
        _mockAppSettings.Setup(_mockAppSettings => _mockAppSettings.LastRefreshTokenTime).Returns(DateTime.UtcNow.AddMinutes(-61));
        _mockAppSettings.SetupProperty(x => x.AccessToken);
        _mockAppSettings.SetupProperty(x => x.LastRefreshTokenTime);

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _mockAppSettings.Object.TokenUrl);
        tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", It.IsAny<string>());
        tokenRequest.Headers.TryAddWithoutValidation("User-Agent", _mockAppSettings.Object.UserAgent);
        tokenRequest.Content = It.IsAny<FormUrlEncodedContent>();

        var responseContent = @"{ ""access_token"": ""new_access_token"" }";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(response);
        var httpClient = new HttpClient(mockHandler.Object);
        httpClient.BaseAddress = new Uri("http://test.com");
        _redditAuthService = new RedditAuthService(_mockAppSettings.Object, httpClient);

        // Act
        await _redditAuthService.RefreshAccessTokenIfNeeded();

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
        _mockAppSettings.VerifySet(x => x.AccessToken = "new_access_token", Times.Once);
        _mockAppSettings.VerifySet(x => x.LastRefreshTokenTime = It.IsAny<DateTime>(), Times.Once);
    }

    [TestMethod]
    public async Task RefreshAccessTokenIfNeeded_WithInvalidRefreshToken_ShouldNotRefreshAccessToken()
    {
        // Arrange
        _mockAppSettings.Setup(x => x.RefreshToken).Returns(string.Empty);

        // Act
        await _redditAuthService.RefreshAccessTokenIfNeeded();

        // Assert
        _mockAppSettings.VerifySet(x => x.AccessToken = It.IsAny<string>(), Times.Never);
        _mockAppSettings.VerifySet(x => x.LastRefreshTokenTime = It.IsAny<DateTime>(), Times.Never);
    }

    [TestMethod]
    public async Task RefreshAccessTokenIfNeeded_WithRecentRefreshToken_ShouldNotRefreshAccessToken()
    {
        // Arrange
        _mockAppSettings.Setup(x => x.RefreshToken).Returns("valid_refresh_token");
        _mockAppSettings.Setup(x => x.LastRefreshTokenTime).Returns(DateTime.UtcNow);

        // Act
        await _redditAuthService.RefreshAccessTokenIfNeeded();

        // Assert
        _mockAppSettings.VerifySet(x => x.AccessToken = It.IsAny<string>(), Times.Never);
        _mockAppSettings.VerifySet(x => x.LastRefreshTokenTime = It.IsAny<DateTime>(), Times.Never);
    }
}