using System.Text;

using PostmanCloneLibrary.Models;

namespace PostmanCloneLibrary.Helper;

public static class CodeSnippetGenerator
{
    public static string ToCurl(RequestTab request)
    {
        var sb = new StringBuilder();
        sb.Append("curl --request ").Append(request.Method.ToUpperInvariant());
        sb.Append(" \\\n  --url ").Append(Q(BuildUrl(request)));

        foreach (var h in Enabled(request.Headers))
            sb.Append(" \\\n  --header ").Append(Q($"{h.Key}: {h.Value}"));

        AppendCurlBody(sb, request);
        return sb.ToString();
    }

    public static string ToPythonRequests(RequestTab request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import requests");
        sb.AppendLine();
        sb.Append("url = ").AppendLine(Py(BuildUrl(request)));

        var headers = Enabled(request.Headers).ToList();
        if (headers.Count > 0)
        {
            sb.AppendLine("headers = {");
            foreach (var h in headers)
                sb.Append("    ").Append(Py(h.Key)).Append(": ").Append(Py(h.Value)).AppendLine(",");
            sb.AppendLine("}");
        }

        var hasBody = false;
        switch (request.BodyType)
        {
            case "json" when !string.IsNullOrWhiteSpace(request.Body):
                sb.Append("payload = ").AppendLine(Py(request.Body));
                hasBody = true;
                break;
            case "text" or "xml" when !string.IsNullOrWhiteSpace(request.Body):
                sb.Append("payload = ").AppendLine(Py(request.Body));
                hasBody = true;
                break;
            case "form":
                sb.AppendLine("payload = {");
                foreach (var f in Enabled(request.FormData))
                    sb.Append("    ").Append(Py(f.Key)).Append(": ").Append(Py(f.Value)).AppendLine(",");
                sb.AppendLine("}");
                hasBody = true;
                break;
            case "multipart":
                sb.AppendLine("files = {");
                foreach (var f in Enabled(request.FormData))
                {
                    if (f.Type == "file")
                        sb.Append("    ").Append(Py(f.Key)).Append(": open(").Append(Py(f.FilePath)).AppendLine(", 'rb'),");
                    else
                        sb.Append("    ").Append(Py(f.Key)).Append(": (None, ").Append(Py(f.Value)).AppendLine("),");
                }
                sb.AppendLine("}");
                break;
        }

        sb.Append("response = requests.request(").Append(Py(request.Method.ToUpperInvariant()));
        sb.Append(", url");
        if (headers.Count > 0) sb.Append(", headers=headers");
        if (request.BodyType == "multipart") sb.Append(", files=files");
        else if (hasBody) sb.Append(request.BodyType == "form" ? ", data=payload" : ", data=payload");
        sb.AppendLine(")");
        sb.AppendLine("print(response.text)");
        return sb.ToString();
    }

    public static string ToJsFetch(RequestTab request)
    {
        var sb = new StringBuilder();
        sb.Append("fetch(").Append(Js(BuildUrl(request))).AppendLine(", {");
        sb.Append("  method: ").Append(Js(request.Method.ToUpperInvariant())).AppendLine(",");

        var headers = Enabled(request.Headers).ToList();
        if (headers.Count > 0)
        {
            sb.AppendLine("  headers: {");
            foreach (var h in headers)
                sb.Append("    ").Append(Js(h.Key)).Append(": ").Append(Js(h.Value)).AppendLine(",");
            sb.AppendLine("  },");
        }

        switch (request.BodyType)
        {
            case "json" when !string.IsNullOrWhiteSpace(request.Body):
                sb.Append("  body: ").Append(Js(request.Body)).AppendLine();
                break;
            case "text" or "xml" when !string.IsNullOrWhiteSpace(request.Body):
                sb.Append("  body: ").Append(Js(request.Body)).AppendLine();
                break;
            case "form":
            {
                var pairs = Enabled(request.FormData)
                    .Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}");
                sb.Append("  body: ").Append(Js(string.Join("&", pairs))).AppendLine();
                break;
            }
            case "multipart":
                sb.AppendLine("  // multipart: build a FormData instance and append fields/files before calling fetch");
                sb.AppendLine("  body: formData");
                break;
        }

        sb.AppendLine("})");
        sb.AppendLine("  .then(res => res.text())");
        sb.AppendLine("  .then(console.log);");
        return sb.ToString();
    }

    private static void AppendCurlBody(StringBuilder sb, RequestTab request)
    {
        switch (request.BodyType)
        {
            case "json" or "text" or "xml" when !string.IsNullOrWhiteSpace(request.Body):
                sb.Append(" \\\n  --data ").Append(Q(request.Body));
                break;
            case "form":
            {
                var body = string.Join("&", Enabled(request.FormData)
                    .Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}"));
                if (body.Length > 0)
                    sb.Append(" \\\n  --data ").Append(Q(body));
                break;
            }
            case "multipart":
                foreach (var f in Enabled(request.FormData))
                {
                    var value = f.Type == "file" ? $"@{f.FilePath}" : f.Value;
                    sb.Append(" \\\n  --form ").Append(Q($"{f.Key}={value}"));
                }
                break;
        }
    }

    private static string BuildUrl(RequestTab request)
    {
        var active = Enabled(request.QueryParams).ToList();
        if (active.Count == 0) return request.Url;
        var sep = request.Url.Contains('?') ? "&" : "?";
        var qs = string.Join("&", active.Select(q =>
            $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value ?? string.Empty)}"));
        return request.Url + sep + qs;
    }

    private static IEnumerable<Models.KeyValuePair> Enabled(IEnumerable<Models.KeyValuePair> items)
        => items.Where(i => i.IsEnabled && !string.IsNullOrWhiteSpace(i.Key));

    private static string Q(string value)
    {
        value ??= string.Empty;
        return "'" + value.Replace("'", "'\\''") + "'";
    }

    private static string Py(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r") + "\"";

    private static string Js(string value) => Py(value);
}
