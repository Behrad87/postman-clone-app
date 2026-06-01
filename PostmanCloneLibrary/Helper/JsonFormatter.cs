namespace PostmanCloneLibrary.Helper;

using Newtonsoft.Json.Linq;

using System.Xml;


public static class JsonFormatter
{
    public static string TryFormat(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        try
        {
            var token = JToken.Parse(json);
            return token.ToString((Newtonsoft.Json.Formatting) Formatting.Indented);
        }
        catch { return json; }
    }

    public static bool IsJson(string contentType)
        => contentType.Contains("json", StringComparison.OrdinalIgnoreCase);

    public static bool IsXml(string contentType)
        => contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);

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