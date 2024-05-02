using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostmanCloneLibrary;

public class ApiAccess : IApiAccess
{
    private readonly HttpClient client = new();
    public async Task<string> CallApiAsync(
        string url,
        bool formatOutput = true,
        HttpAction action = HttpAction.GET)
    {
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            if (formatOutput)
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                json = JsonSerializer.Serialize(jsonElement, options: new JsonSerializerOptions { WriteIndented = true });

            }
            return json;
        }
        else
        {
            return $"Error: {response.StatusCode}";
        }

    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        bool output = Uri.TryCreate(url, UriKind.Absolute, out var uriOutput) &&
            (uriOutput.Scheme == Uri.UriSchemeHttps);


        return output;
    }
}
