using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;
using System.Collections.ObjectModel;
using Xunit;

namespace PostmanCloneLibrary.Tests;

public class CodeSnippetGeneratorTests
{
    [Fact]
    public void ToCurl_GeneratesValidCurlCommand()
    {
        var tab = new RequestTab
        {
            Method = "POST",
            Url = "https://httpbin.org/post",
            BodyType = "json",
            Body = "{\"message\":\"hello\"}",
            Headers = new ObservableCollection<Models.KeyValuePair>
            {
                new() { Key = "X-Test-Header", Value = "TestValue", IsEnabled = true }
            }
        };

        var curl = CodeSnippetGenerator.ToCurl(tab);

        Assert.Contains("curl --request POST", curl);
        Assert.Contains("--url 'https://httpbin.org/post'", curl);
        Assert.Contains("--header 'X-Test-Header: TestValue'", curl);
        Assert.Contains("--data '{\"message\":\"hello\"}'", curl);
    }

    [Fact]
    public void ToPythonRequests_GeneratesValidPythonSnippet()
    {
        var tab = new RequestTab
        {
            Method = "GET",
            Url = "https://httpbin.org/get",
            Headers = new ObservableCollection<Models.KeyValuePair>
            {
                new() { Key = "Accept", Value = "application/json", IsEnabled = true }
            }
        };

        var py = CodeSnippetGenerator.ToPythonRequests(tab);

        Assert.Contains("import requests", py);
        Assert.Contains("url = \"https://httpbin.org/get\"", py);
        Assert.Contains("\"Accept\": \"application/json\"", py);
        Assert.Contains("requests.request(\"GET\", url, headers=headers)", py);
    }

    [Fact]
    public void ToJsFetch_GeneratesValidFetchSnippet()
    {
        var tab = new RequestTab
        {
            Method = "POST",
            Url = "https://httpbin.org/post",
            BodyType = "json",
            Body = "{\"name\":\"Alice\"}"
        };

        var js = CodeSnippetGenerator.ToJsFetch(tab);

        Assert.Contains("fetch(\"https://httpbin.org/post\"", js);
        Assert.Contains("method: \"POST\"", js);
        Assert.Contains("body: \"{\\\"name\\\":\\\"Alice\\\"}\"", js);
    }
}
