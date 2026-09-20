using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.ImportExport;
using PostmanCloneLibrary.Models;
using Xunit;

namespace PostmanCloneLibrary.Tests;

public class JsonFormatterAndConverterTests
{
    [Fact]
    public void TryFormat_ValidJson_FormatsWithIndentation()
    {
        var raw = "{\"id\":1,\"name\":\"test\"}";
        var formatted = JsonFormatter.TryFormat(raw);

        Assert.Contains("\n", formatted);
        Assert.Contains("  \"id\": 1", formatted);
        Assert.Contains("  \"name\": \"test\"", formatted);
    }

    [Fact]
    public void TryFormat_InvalidJson_ReturnsOriginal()
    {
        var invalid = "not valid json {";
        var result = JsonFormatter.TryFormat(invalid);

        Assert.Equal(invalid, result);
    }

    [Fact]
    public void FormatSize_ReturnsReadableStrings()
    {
        Assert.Equal("500 B", JsonFormatter.FormatSize(500));
        Assert.Equal("1.5 KB", JsonFormatter.FormatSize(1536));
        Assert.Equal("2.00 MB", JsonFormatter.FormatSize(2097152));
    }

    [Fact]
    public void CollectionConverter_RoundTrip_PreservesRequests()
    {
        var collection = new Collection
        {
            Name = "My API Test Suite"
        };
        collection.Requests.Add(new SavedRequest
        {
            Name = "Get Users",
            Method = "GET",
            Url = "https://jsonplaceholder.typicode.com/users"
        });
        collection.Requests.Add(new SavedRequest
        {
            Name = "Create Post",
            Method = "POST",
            Url = "https://jsonplaceholder.typicode.com/posts"
        });

        var json = PostmanCollectionConverter.Export(collection);
        Assert.NotNull(json);

        var imported = PostmanCollectionConverter.Import(json);
        Assert.Equal("My API Test Suite", imported.Name);
        Assert.Equal(2, imported.Requests.Count);
        Assert.Equal("Get Users", imported.Requests[0].Name);
        Assert.Equal("GET", imported.Requests[0].Method);
        Assert.Equal("https://jsonplaceholder.typicode.com/users", imported.Requests[0].Url);
        Assert.Equal("Create Post", imported.Requests[1].Name);
        Assert.Equal("POST", imported.Requests[1].Method);
        Assert.Equal("https://jsonplaceholder.typicode.com/posts", imported.Requests[1].Url);
    }
}

public class ApiAccessTests
{
    [Theory]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://api.github.com/users", true)]
    [InlineData("http://127.0.0.1:8080/api?v=1", true)]
    [InlineData("ftp://ftp.example.com", false)]
    [InlineData("not a url", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsValidUrl_ValidatesCorrectly(string url, bool expected)
    {
        using var api = new ApiAccess();
        var actual = api.IsValidUrl(url);
        Assert.Equal(expected, actual);
    }
}
