using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Services;

public interface IRedditAuthService
{
    /// <summary>
    /// Refreshes the access token if needed.
    /// </summary>
    Task RefreshAccessTokenIfNeeded();
}

public class RedditAuthService : IRedditAuthService
{
    private readonly AppSettings _appSettings;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedditAuthService"/> class.
    /// </summary>
    /// <param name="appSettings">The app settings.</param>
    public RedditAuthService(AppSettings appSettings, HttpClient client)
    {
        _appSettings = appSettings;
        _client = client;
    }

    /// <summary>
    /// Refreshes the access token if needed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RefreshAccessTokenIfNeeded()
    {
        if (!string.IsNullOrEmpty(_appSettings.RefreshToken) && (!_appSettings.LastRefreshTokenTime.HasValue || (DateTime.UtcNow - _appSettings.LastRefreshTokenTime.Value).TotalMinutes > 60))
        {
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, _appSettings.TokenUrl);
            string? encodedCredentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_appSettings.AppId}:{_appSettings.AppSecret}"));
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            tokenRequest.Headers.TryAddWithoutValidation("User-Agent", _appSettings.UserAgent);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", _appSettings.RefreshToken ?? "" }
                });

            var response = await _client.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JObject.Parse(responseContent);
            _appSettings.AccessToken = jsonResponse.access_token;
            _appSettings.LastRefreshTokenTime = DateTime.UtcNow;
        }
    }
}