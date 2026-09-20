using System.Text;

using PostmanCloneLibrary.Models;

namespace PostmanCloneLibrary.Helper;

public static class AuthApplier
{
    public static void Apply(RequestTab request)
    {
        var auth = request.Auth;
        if (auth is null) return;

        switch (auth.Type?.ToLowerInvariant())
        {
            case "bearer" when !string.IsNullOrWhiteSpace(auth.BearerToken):
                UpsertHeader(request, "Authorization", $"Bearer {auth.BearerToken}");
                break;

            case "basic" when !string.IsNullOrEmpty(auth.BasicUsername) || !string.IsNullOrEmpty(auth.BasicPassword):
            {
                var raw = $"{auth.BasicUsername}:{auth.BasicPassword}";
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                UpsertHeader(request, "Authorization", $"Basic {token}");
                break;
            }

            case "apikey" when !string.IsNullOrWhiteSpace(auth.ApiKeyName):
                if (string.Equals(auth.ApiKeyLocation, "query", StringComparison.OrdinalIgnoreCase))
                    UpsertQuery(request, auth.ApiKeyName, auth.ApiKeyValue);
                else
                    UpsertHeader(request, auth.ApiKeyName, auth.ApiKeyValue);
                break;
        }
    }

    private static void UpsertHeader(RequestTab request, string key, string value)
    {
        var existing = request.Headers.FirstOrDefault(h =>
            h.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.Value = value;
            existing.IsEnabled = true;
        }
        else
        {
            request.Headers.Add(new Models.KeyValuePair
            {
                Key = key,
                Value = value,
                IsEnabled = true
            });
        }
    }

    private static void UpsertQuery(RequestTab request, string key, string value)
    {
        var existing = request.QueryParams.FirstOrDefault(q =>
            q.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.Value = value;
            existing.IsEnabled = true;
        }
        else
        {
            request.QueryParams.Add(new Models.KeyValuePair
            {
                Key = key,
                Value = value,
                IsEnabled = true
            });
        }
    }
}
