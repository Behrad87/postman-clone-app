namespace PostmanCloneLibrary.Models;

public class AuthConfig
{
    /// <summary>none | bearer | basic | apikey</summary>
    public string Type { get; set; } = "none";

    public string BearerToken { get; set; } = string.Empty;

    public string BasicUsername { get; set; } = string.Empty;
    public string BasicPassword { get; set; } = string.Empty;

    public string ApiKeyName { get; set; } = "X-API-Key";
    public string ApiKeyValue { get; set; } = string.Empty;
    /// <summary>header | query</summary>
    public string ApiKeyLocation { get; set; } = "header";

    // TODO: OAuth2 (authorization code / client credentials) — not implemented.

    public AuthConfig Clone() => new()
    {
        Type = Type,
        BearerToken = BearerToken,
        BasicUsername = BasicUsername,
        BasicPassword = BasicPassword,
        ApiKeyName = ApiKeyName,
        ApiKeyValue = ApiKeyValue,
        ApiKeyLocation = ApiKeyLocation
    };
}
