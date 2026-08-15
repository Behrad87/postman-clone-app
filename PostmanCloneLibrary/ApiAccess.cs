using PostmanCloneLibrary.Models;

using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace PostmanCloneLibrary;

public sealed class ApiAccess : IApiAccess, IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpClient _insecureClient;

    public ApiAccess()
    {
        _client = CreateClient(insecure: false);
        _insecureClient = CreateClient(insecure: true);
    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

    public async Task<ResponseData> SendAsync(RequestTab request, CancellationToken ct = default)
    {
        var client = request.DisableSslVerification ? _insecureClient : _client;
        var url = BuildUrl(request.Url, request.QueryParams);
        using var req = new HttpRequestMessage(new HttpMethod(request.Method), url);

        foreach (var h in request.Headers.Where(h => h.IsEnabled && !string.IsNullOrWhiteSpace(h.Key)))
        {
            if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;
            req.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        var method = request.Method.ToUpperInvariant();
        if (method is not "HEAD")
            req.Content = BuildBody(request);

        var sw = Stopwatch.StartNew();
        try
        {
            using var response = await client.SendAsync(req, ct);
            sw.Stop();

            var bodyBytes = await response.Content.ReadAsByteArrayAsync(ct);
            var bodyText = Encoding.UTF8.GetString(bodyBytes);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            sw.Stop();
            return TransportError("cancelled", "Request cancelled.", sw.ElapsedMilliseconds);
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            return TransportError("timeout", "The request timed out. The server took too long to respond.", sw.ElapsedMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            var (kind, message) = ClassifyHttpError(ex);
            return TransportError(kind, message, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportError("unknown", $"Unexpected error: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _insecureClient.Dispose();
    }

    private static HttpClient CreateClient(bool insecure)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        };

        if (insecure)
            handler.ServerCertificateCustomValidationCallback = static (_, _, _, _) => true;

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };
    }

    private static HttpContent? BuildBody(RequestTab request)
    {
        switch (request.BodyType)
        {
            case "json" when !string.IsNullOrWhiteSpace(request.Body):
                return new StringContent(request.Body, Encoding.UTF8, "application/json");
            case "text" when !string.IsNullOrWhiteSpace(request.Body):
                return new StringContent(request.Body, Encoding.UTF8, "text/plain");
            case "xml" when !string.IsNullOrWhiteSpace(request.Body):
                return new StringContent(request.Body, Encoding.UTF8, "application/xml");
            case "form":
            {
                var form = request.FormData
                    .Where(f => f.IsEnabled && !string.IsNullOrWhiteSpace(f.Key))
                    .Select(f => new System.Collections.Generic.KeyValuePair<string, string>(f.Key, f.Value));
                return new FormUrlEncodedContent(form);
            }
            case "multipart":
            {
                var content = new MultipartFormDataContent();
                foreach (var f in request.FormData.Where(f => f.IsEnabled && !string.IsNullOrWhiteSpace(f.Key)))
                {
                    if (f.Type == "file" && !string.IsNullOrWhiteSpace(f.FilePath) && File.Exists(f.FilePath))
                    {
                        var stream = File.OpenRead(f.FilePath);
                        var fileContent = new StreamContent(stream);
                        content.Add(fileContent, f.Key, Path.GetFileName(f.FilePath));
                    }
                    else
                    {
                        content.Add(new StringContent(f.Value ?? string.Empty), f.Key);
                    }
                }
                return content;
            }
            default:
                return null;
        }
    }

    private static string BuildUrl(string baseUrl, IEnumerable<Models.KeyValuePair> queryParams)
    {
        var active = queryParams.Where(q => q.IsEnabled && !string.IsNullOrWhiteSpace(q.Key)).ToList();
        if (active.Count == 0) return baseUrl;

        var sep = baseUrl.Contains('?') ? "&" : "?";
        var qs = string.Join("&", active.Select(q =>
            $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value ?? string.Empty)}"));
        return baseUrl + sep + qs;
    }

    private static (string Kind, string Message) ClassifyHttpError(HttpRequestException ex)
    {
        switch (ex.HttpRequestError)
        {
            case HttpRequestError.NameResolutionError:
                return ("dns", "Could not resolve the host name. Check the URL and your DNS / network connection.");
            case HttpRequestError.ConnectionError:
                return ("connection", "Could not connect to the server. Is it running, and is the host/port correct?");
            case HttpRequestError.SecureConnectionError:
                return ("ssl", "TLS certificate validation failed. Enable “Disable SSL verification” only if you trust this host.");
            case HttpRequestError.ProxyTunnelError:
                return ("connection", "Could not connect through the configured proxy.");
        }

        if (ex.InnerException is SocketException sock)
        {
            return sock.SocketErrorCode switch
            {
                SocketError.HostNotFound or SocketError.NoData or SocketError.TryAgain
                    => ("dns", "Could not resolve the host name. Check the URL and your DNS / network connection."),
                SocketError.ConnectionRefused
                    => ("connection", "Connection refused. Is the server running on that host and port?"),
                SocketError.TimedOut
                    => ("timeout", "The connection timed out."),
                SocketError.NetworkUnreachable or SocketError.HostUnreachable
                    => ("connection", "The host is unreachable. Check your network connection."),
                _ => ("connection", $"Could not connect: {sock.Message}")
            };
        }

        if (ex.InnerException is AuthenticationException
            || ex.Message.Contains("certificate", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("SSL", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase))
        {
            return ("ssl", "TLS certificate validation failed. Enable “Disable SSL verification” only if you trust this host.");
        }

        var fallback = string.IsNullOrWhiteSpace(ex.Message)
            ? "Could not complete the request."
            : ex.Message;
        return ("connection", fallback);
    }

    private static ResponseData TransportError(string kind, string message, long elapsedMs) => new()
    {
        StatusCode = 0,
        StatusText = kind switch
        {
            "timeout" => "Timeout",
            "cancelled" => "Cancelled",
            "dns" => "DNS Error",
            "ssl" => "TLS Error",
            "connection" => "Connection Error",
            _ => "Error"
        },
        ElapsedMs = elapsedMs,
        IsSuccess = false,
        IsTransportError = true,
        ErrorKind = kind,
        FriendlyError = message,
        Body = message
    };
}
