using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;
using Xunit;

namespace PostmanCloneLibrary.Tests;

public class AuthApplierTests
{
    [Fact]
    public void Apply_BearerToken_AddsAuthorizationHeader()
    {
        var tab = new RequestTab
        {
            Auth = new AuthConfig
            {
                Type = "bearer",
                BearerToken = "eyJhGciOi..."
            }
        };

        AuthApplier.Apply(tab);

        var header = Assert.Single(tab.Headers);
        Assert.Equal("Authorization", header.Key);
        Assert.Equal("Bearer eyJhGciOi...", header.Value);
    }

    [Fact]
    public void Apply_BasicAuth_WithCredentials_AddsBase64Header()
    {
        var tab = new RequestTab
        {
            Auth = new AuthConfig
            {
                Type = "basic",
                BasicUsername = "admin",
                BasicPassword = "password123"
            }
        };

        AuthApplier.Apply(tab);

        var header = Assert.Single(tab.Headers);
        Assert.Equal("Authorization", header.Key);
        // admin:password123 base64 is YWRtaW46cGFzc3dvcmQxMjM=
        Assert.Equal("Basic YWRtaW46cGFzc3dvcmQxMjM=", header.Value);
    }

    [Fact]
    public void Apply_BasicAuth_EmptyCredentials_DoesNotAddHeader()
    {
        var tab = new RequestTab
        {
            Auth = new AuthConfig
            {
                Type = "basic",
                BasicUsername = "",
                BasicPassword = ""
            }
        };

        AuthApplier.Apply(tab);

        // Before our fix, this would incorrectly add "Authorization: Basic Og=="
        Assert.Empty(tab.Headers);
    }

    [Fact]
    public void Apply_ApiKey_InHeader_AddsHeader()
    {
        var tab = new RequestTab
        {
            Auth = new AuthConfig
            {
                Type = "apikey",
                ApiKeyName = "X-Api-Key",
                ApiKeyValue = "my-secret-key",
                ApiKeyLocation = "header"
            }
        };

        AuthApplier.Apply(tab);

        var header = Assert.Single(tab.Headers);
        Assert.Equal("X-Api-Key", header.Key);
        Assert.Equal("my-secret-key", header.Value);
    }

    [Fact]
    public void Apply_ApiKey_InQuery_AddsQueryParam()
    {
        var tab = new RequestTab
        {
            Auth = new AuthConfig
            {
                Type = "apikey",
                ApiKeyName = "api_key",
                ApiKeyValue = "query123",
                ApiKeyLocation = "query"
            }
        };

        AuthApplier.Apply(tab);

        Assert.Empty(tab.Headers);
        var param = Assert.Single(tab.QueryParams);
        Assert.Equal("api_key", param.Key);
        Assert.Equal("query123", param.Value);
    }
}
