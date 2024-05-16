namespace Services;

public class AppSettings
{
    public string? AppSecret { get; set; }
    public string? AppCode { get; set; }
    public string? AppId { get; set; }
    public virtual string? RefreshToken { get; set; }
    public virtual string? AccessToken { get; set; }
    public virtual DateTime? LastRefreshTokenTime { get; set; }
    public string? RedditBaseUrl { get; set; }
    public string? TokenUrl { get; set; }
    public string? UserAgent { get; set; }
}