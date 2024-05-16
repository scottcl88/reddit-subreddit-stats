using Moq;
using Services;

namespace Tests;

[TestClass]
public class SubRedditServiceTests
{
    [TestMethod]
    public async Task GetPostsByUpVotes_ReturnsExpectedPosts()
    {
        // Arrange
        var baseRedditServiceMock = new Mock<IBaseRedditService>();
        baseRedditServiceMock.Setup(x => x.GetTopPosts(It.IsAny<string>()))
                             .ReturnsAsync(GetMockPosts());

        var subRedditService = new SubRedditService(baseRedditServiceMock.Object);

        // Act
        var result = await subRedditService.GetPostsByUpVotes("testsubreddit");

        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Title1", result[0].Title);
        Assert.AreEqual(10, result[0].UpVotes);
    }

    [TestMethod]
    public async Task GetTopUsers_ReturnsExpectedTopUsers()
    {
        // Arrange
        var baseRedditServiceMock = new Mock<IBaseRedditService>();
        baseRedditServiceMock.Setup(x => x.GetTopPosts(It.IsAny<string>()))
                             .ReturnsAsync(GetMockPosts());

        var subRedditService = new SubRedditService(baseRedditServiceMock.Object);

        // Act
        var result = await subRedditService.GetTopUsers("testsubreddit");

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("User1", result[0].UserName);
        Assert.AreEqual(2, result[0].PostCount);
    }

    [TestMethod]
    public async Task GetPostFlairDistribution_ReturnsExpectedFlairDistribution()
    {
        // Arrange
        var baseRedditServiceMock = new Mock<IBaseRedditService>();
        baseRedditServiceMock.Setup(x => x.GetNewPosts(It.IsAny<string>()))
                             .ReturnsAsync(GetMockPosts());

        var subRedditService = new SubRedditService(baseRedditServiceMock.Object);

        // Act
        var result = await subRedditService.GetPostFlairDistribution("testsubreddit");

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.ContainsKey("Flair1"));
        Assert.AreEqual(1, result["Flair1"]);
    }

    [TestMethod]
    public async Task GetImageTextPostRatio_ReturnsExpectedRatio()
    {
        // Arrange
        var baseRedditServiceMock = new Mock<IBaseRedditService>();
        baseRedditServiceMock.Setup(x => x.GetNewPosts(It.IsAny<string>()))
                             .ReturnsAsync(GetMockPosts());

        var subRedditService = new SubRedditService(baseRedditServiceMock.Object);

        // Act
        var result = await subRedditService.GetImageTextPostRatio("testsubreddit");

        // Assert
        Assert.AreEqual(2, result.ImagePosts);
        Assert.AreEqual(1, result.TextPosts);
    }

    private Newtonsoft.Json.Linq.JArray GetMockPosts()
    {
        var posts = new List<Newtonsoft.Json.Linq.JObject>
        {
            new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("data",
                new Newtonsoft.Json.Linq.JObject(
                    new Newtonsoft.Json.Linq.JProperty("title", "Title1"),
                    new Newtonsoft.Json.Linq.JProperty("ups", 10),
                    new Newtonsoft.Json.Linq.JProperty("post_hint", "image"),
                    new Newtonsoft.Json.Linq.JProperty("link_flair_text", "Flair1"),
                    new Newtonsoft.Json.Linq.JProperty("author", "User1")
                )
            )),
            new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("data",
                new Newtonsoft.Json.Linq.JObject(
                    new Newtonsoft.Json.Linq.JProperty("title", "Title2"),
                    new Newtonsoft.Json.Linq.JProperty("ups", 5),
                    new Newtonsoft.Json.Linq.JProperty("post_hint", "image"),
                    new Newtonsoft.Json.Linq.JProperty("link_flair_text", "Flair2"),
                    new Newtonsoft.Json.Linq.JProperty("author", "User1")
                )
            )),
            new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("data",
                new Newtonsoft.Json.Linq.JObject(
                    new Newtonsoft.Json.Linq.JProperty("title", "Title3"),
                    new Newtonsoft.Json.Linq.JProperty("ups", 3)
                )
            ))
        };

        return new Newtonsoft.Json.Linq.JArray(posts);
    }
}
