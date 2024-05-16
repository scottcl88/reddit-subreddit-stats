# Reddit Subreddit Stats

The Reddit Subreddit Stats is a web application built to display statistics for a given subreddit. It fetches data from the Reddit API, including top posts, top users, post flair distribution, and image vs text post ratio.

## Technologies Used

- C# (ASP.NET Core)
- JavaScript (jQuery)
- HTML (with Razor)
- CSS

## How to Use

1. Clone the repository to your local machine.
2. Update the `appsettings.json` file or use User-Secrets to provide your Reddit API credentials. These credentials are necessary for authenticating with the Reddit API.
3. Build and run the application using Visual Studio or your preferred IDE.
4. Enter the name of the subreddit you want to get statistics for and click the "Fetch Data" button.
5. The application will display the requested statistics, including top posts, top users, post flair distribution, and image vs text post ratio.

## Configuration

Before running the application, make sure to update the `appsettings.json` file or use User-Secrets to provide your Reddit API credentials. You can obtain these credentials by creating a Reddit developer account and registering a new application to obtain the necessary client ID and client secret.

```json
{
  "AppSettings": {
    "AppId": "YOUR_APP_ID",
    "AppSecret": "YOUR_APP_SECRET",
    "AppCode": "YOUR_APP_CODE",
    "RefreshToken": "YOUR_REFRESH_TOKEN",
    "UserAgent": "YOUR_USER_AGENT",
    "AccessToken": "YOUR_ACCESS_TOKEN"
  }
}
```

Replace `YOUR_APP_ID`, `YOUR_APP_SECRET`, `YOUR_APP_CODE`, `YOUR_USER_AGENT`, and `YOUR_REFRESH_TOKEN` with your actual Reddit API credentials and user agent string.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
