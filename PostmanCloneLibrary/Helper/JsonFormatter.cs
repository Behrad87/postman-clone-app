using System.Xml.Linq;

using Newtonsoft.Json.Linq;

namespace PostmanCloneLibrary.Helper;

public static class JsonFormatter
{
    public static string TryFormat(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        try
        {
            var token = JToken.Parse(json);
            return token.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        catch
        {
            return json;
        }
    }

    public static string TryFormatXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return xml;
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString();
        }
        catch
        {
            return xml;
        }
    }

    public static bool IsJson(string contentType)
        => !string.IsNullOrEmpty(contentType) && contentType.Contains("json", StringComparison.OrdinalIgnoreCase);

    public static bool LooksLikeJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return false;
        var t = body.TrimStart();
        return t.StartsWith('{') || t.StartsWith('[');
    }

    public static bool IsXml(string contentType)
        => !string.IsNullOrEmpty(contentType) && contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);

    public static bool IsBinary(string contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("octet-stream", StringComparison.OrdinalIgnoreCase);
    }

    public static string GetStatusCategory(int code) => code switch
    {
        >= 100 and < 200 => "1xx",
        >= 200 and < 300 => "2xx",
        >= 300 and < 400 => "3xx",
        >= 400 and < 500 => "4xx",
        >= 500 => "5xx",
        _ => "error"
    };

    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F2} MB"
    };
}
