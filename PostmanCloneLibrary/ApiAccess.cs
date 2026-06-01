using PostmanCloneLibrary.Models;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostmanCloneLibrary;

public class ApiAccess : IApiAccess
{
    private readonly HttpClient? _client;

    public ApiAccess(HttpClient? client=null)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        _client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };
    }

    public async Task<string> CallApiAsync(
        string url,
        bool formatOutput = true,
        HttpAction action = HttpAction.GET)
    {
        var response = await _client.GetAsync(url);

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


    public async Task<ResponseData> SendAsync(RequestTab request, CancellationToken ct = default)
    {
        var url = BuildUrl(request.Url, request.QueryParams);
        using var req = new HttpRequestMessage(new HttpMethod(request.Method), url);

        // Add headers
        foreach (var h in request.Headers.Where(h => h.IsEnabled && !string.IsNullOrWhiteSpace(h.Key)))
            req.Headers.TryAddWithoutValidation(h.Key, h.Value);

        // Add body
        if (request.BodyType == "json" && !string.IsNullOrWhiteSpace(request.Body))
        {
            req.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }
        else if (request.BodyType == "text" && !string.IsNullOrWhiteSpace(request.Body))
        {
            req.Content = new StringContent(request.Body, Encoding.UTF8, "text/plain");
        }
        else if (request.BodyType == "xml" && !string.IsNullOrWhiteSpace(request.Body))
        {
            req.Content = new StringContent(request.Body, Encoding.UTF8, "application/xml");
        }
        else if (request.BodyType == "form")
        {
            var form = request.FormData
                .Where(f => f.IsEnabled && !string.IsNullOrWhiteSpace(f.Key))
                .Select(f => new System.Collections.Generic.KeyValuePair<string, string>(f.Key, f.Value));
            req.Content = new FormUrlEncodedContent(form);
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _client.SendAsync(req, ct);
            sw.Stop();

            var bodyBytes = await response.Content.ReadAsByteArrayAsync(ct);
            var bodyText = Encoding.UTF8.GetString(bodyBytes);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            var headers = new Dictionary<string, string>();
            foreach (var h in response.Headers)
                headers[h.Key] = string.Join(", ", h.Value);
            foreach (var h in response.Content.Headers)
                headers[h.Key] = string.Join(", ", h.Value);

            return new ResponseData
            {
                StatusCode = (int)response.StatusCode,
                StatusText = response.ReasonPhrase ?? response.StatusCode.ToString(),
                ElapsedMs = sw.ElapsedMilliseconds,
                SizeBytes = bodyBytes.Length,
                Headers = headers,
                Body = bodyText,
                ContentType = contentType,
                IsSuccess = response.IsSuccessStatusCode
            };
        }
        catch (TaskCanceledException)
        {
            return new ResponseData { StatusCode = 0, StatusText = "Request Cancelled", ElapsedMs = sw.ElapsedMilliseconds };
        }
        catch (Exception ex)
        {
            return new ResponseData { StatusCode = 0, StatusText = ex.Message, ElapsedMs = sw.ElapsedMilliseconds };
        }
    }

    private static string BuildUrl(string baseUrl, IEnumerable<Models.KeyValuePair> queryParams)
    {
        var active = queryParams.Where(q => q.IsEnabled && !string.IsNullOrWhiteSpace(q.Key)).ToList();
        if (!active.Any()) return baseUrl;

        var sep = baseUrl.Contains('?') ? "&" : "?";
        var qs = string.Join("&", active.Select(q => $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value)}"));
        return baseUrl + sep + qs;
    }


}
