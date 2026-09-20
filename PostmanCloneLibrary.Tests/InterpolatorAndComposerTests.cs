using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;
using System.Collections.ObjectModel;
using Xunit;

namespace PostmanCloneLibrary.Tests;

public class InterpolatorAndComposerTests
{
    [Fact]
    public void Interpolate_ReplacesVariablesCorrectly()
    {
        var vars = new Dictionary<string, string>
        {
            ["host"] = "api.example.com",
            ["id"] = "42"
        };
        var unresolved = new List<string>();

        var result = VariableInterpolator.Interpolate("https://{{host}}/users/{{id}}", vars, unresolved);

        Assert.Equal("https://api.example.com/users/42", result);
        Assert.Empty(unresolved);
    }

    [Fact]
    public void Interpolate_CapturesUnresolvedVariables()
    {
        var vars = new Dictionary<string, string>
        {
            ["host"] = "api.example.com"
        };
        var unresolved = new List<string>();

        var result = VariableInterpolator.Interpolate("https://{{host}}/users/{{missingVar}}", vars, unresolved);

        Assert.Equal("https://api.example.com/users/{{missingVar}}", result);
        Assert.Single(unresolved);
        Assert.Contains("missingVar", unresolved);
    }

    [Fact]
    public void RequestComposer_PrependsProtocol_WhenVariableResolvesToHostWithoutScheme()
    {
        var vars = new Dictionary<string, string>
        {
            ["baseUrl"] = "localhost:5000"
        };
        var tab = new RequestTab
        {
            Method = "GET",
            Url = "{{baseUrl}}/api/items"
        };

        var composed = RequestComposer.Compose(tab, vars);

        // Before our fix, this would stay "localhost:5000/api/items" and fail URL validation!
        Assert.Equal("http://localhost:5000/api/items", composed.Request.Url);
        Assert.Empty(composed.UnresolvedVariables);
    }

    [Fact]
    public void RequestComposer_PreservesHttpsScheme()
    {
        var vars = new Dictionary<string, string>
        {
            ["baseUrl"] = "https://api.github.com"
        };
        var tab = new RequestTab
        {
            Method = "GET",
            Url = "{{baseUrl}}/users"
        };

        var composed = RequestComposer.Compose(tab, vars);

        Assert.Equal("https://api.github.com/users", composed.Request.Url);
    }

    [Fact]
    public void RequestComposer_InterpolatesHeadersAndBody()
    {
        var vars = new Dictionary<string, string>
        {
            ["token"] = "secret123",
            ["userId"] = "99"
        };
        var tab = new RequestTab
        {
            Method = "POST",
            Url = "https://api.example.com/data",
            BodyType = "json",
            Body = "{ \"user\": {{userId}} }",
            Headers = new ObservableCollection<Models.KeyValuePair>
            {
                new() { Key = "X-Auth", Value = "{{token}}", IsEnabled = true }
            }
        };

        var composed = RequestComposer.Compose(tab, vars);

        Assert.Equal("{ \"user\": 99 }", composed.Request.Body);
        Assert.Equal("secret123", composed.Request.Headers[0].Value);
    }
}
