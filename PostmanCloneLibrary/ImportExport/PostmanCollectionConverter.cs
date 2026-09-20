using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

using PostmanCloneLibrary.Models;

using AppCollection = PostmanCloneLibrary.Models.Collection;

namespace PostmanCloneLibrary.ImportExport;

public static class PostmanCollectionConverter
{
    private const string SchemaV21 = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json";

    public static AppCollection Import(string json)
    {
        var root = JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidDataException("Not a valid JSON document.");

        var info = root["info"]?.AsObject();
        var name = info?["name"]?.GetValue<string>() ?? "Imported Collection";

        var collection = new AppCollection { Name = name };
        FlattenItems(root["item"] as JsonArray, collection.Requests, parentPrefix: null);
        return collection;
    }

    public static string Export(AppCollection collection)
    {
        var items = new JsonArray();
        foreach (var saved in collection.Requests)
            items.Add(ExportItem(saved));

        var root = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["_postman_id"] = collection.Id.ToString(),
                ["name"] = collection.Name,
                ["schema"] = SchemaV21
            },
            ["item"] = items
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static void FlattenItems(JsonArray? items, ObservableCollection<SavedRequest> target, string? parentPrefix)
    {
        if (items is null) return;

        foreach (var node in items)
        {
            if (node is not JsonObject obj) continue;

            var name = obj["name"]?.GetValue<string>() ?? "Untitled";
            var nested = obj["item"] as JsonArray;
            if (nested is not null)
            {
                var prefix = string.IsNullOrEmpty(parentPrefix) ? name : $"{parentPrefix} / {name}";
                FlattenItems(nested, target, prefix);
                continue;
            }

            var requestNode = obj["request"];
            if (requestNode is null) continue;

            var displayName = string.IsNullOrEmpty(parentPrefix) ? name : $"{parentPrefix} / {name}";
            target.Add(ParseRequest(requestNode, displayName));
        }
    }

    private static SavedRequest ParseRequest(JsonNode requestNode, string name)
    {
        var tab = new RequestTab { Name = name };

        if (requestNode is JsonValue scalar)
        {
            tab.Method = "GET";
            tab.Url = scalar.GetValue<string>() ?? string.Empty;
            return Wrap(tab);
        }

        var obj = requestNode.AsObject();
        tab.Method = obj["method"]?.GetValue<string>() ?? "GET";
        tab.Url = ReadUrl(obj["url"]);
        ReadHeaders(obj["header"] as JsonArray, tab.Headers);
        ReadAuth(obj["auth"] as JsonObject, tab);
        ReadBody(obj["body"] as JsonObject, tab);

        if (obj["url"] is JsonObject urlObj && urlObj["query"] is JsonArray query)
            ReadKeyValues(query, tab.QueryParams);

        return Wrap(tab);
    }

    private static SavedRequest Wrap(RequestTab tab) => new()
    {
        Name = tab.Name,
        Method = tab.Method,
        Url = tab.Url,
        RequestData = tab
    };

    private static string ReadUrl(JsonNode? urlNode)
    {
        if (urlNode is null) return string.Empty;
        if (urlNode is JsonValue v) return v.GetValue<string>() ?? string.Empty;
        return urlNode["raw"]?.GetValue<string>() ?? string.Empty;
    }

    private static void ReadHeaders(JsonArray? headers, ObservableCollection<Models.KeyValuePair> target)
    {
        if (headers is null) return;
        ReadKeyValues(headers, target);
    }

    private static void ReadKeyValues(JsonArray items, ObservableCollection<Models.KeyValuePair> target)
    {
        foreach (var node in items)
        {
            if (node is not JsonObject obj) continue;
            var key = obj["key"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key)) continue;
            target.Add(new Models.KeyValuePair
            {
                Key = key,
                Value = obj["value"]?.GetValue<string>() ?? string.Empty,
                Description = obj["description"]?.GetValue<string>() ?? string.Empty,
                IsEnabled = obj["disabled"]?.GetValue<bool>() != true
            });
        }
    }

    private static void ReadAuth(JsonObject? auth, RequestTab tab)
    {
        if (auth is null) return;
        var type = auth["type"]?.GetValue<string>()?.ToLowerInvariant() ?? "none";
        tab.Auth.Type = type switch
        {
            "bearer" => "bearer",
            "basic" => "basic",
            "apikey" => "apikey",
            _ => "none"
        };

        switch (tab.Auth.Type)
        {
            case "bearer":
                tab.Auth.BearerToken = FindAuthValue(auth["bearer"] as JsonArray, "token");
                break;
            case "basic":
                tab.Auth.BasicUsername = FindAuthValue(auth["basic"] as JsonArray, "username");
                tab.Auth.BasicPassword = FindAuthValue(auth["basic"] as JsonArray, "password");
                break;
            case "apikey":
                tab.Auth.ApiKeyName = FindAuthValue(auth["apikey"] as JsonArray, "key");
                tab.Auth.ApiKeyValue = FindAuthValue(auth["apikey"] as JsonArray, "value");
                var loc = FindAuthValue(auth["apikey"] as JsonArray, "in");
                tab.Auth.ApiKeyLocation = loc is "query" ? "query" : "header";
                break;
        }
    }

    private static string FindAuthValue(JsonArray? entries, string key)
    {
        if (entries is null) return string.Empty;
        foreach (var node in entries)
        {
            if (node is not JsonObject obj) continue;
            if (string.Equals(obj["key"]?.GetValue<string>(), key, StringComparison.OrdinalIgnoreCase))
                return obj["value"]?.GetValue<string>() ?? string.Empty;
        }
        return string.Empty;
    }

    private static void ReadBody(JsonObject? body, RequestTab tab)
    {
        if (body is null)
        {
            tab.BodyType = "none";
            return;
        }

        var mode = body["mode"]?.GetValue<string>() ?? "raw";
        switch (mode)
        {
            case "raw":
                tab.Body = body["raw"]?.GetValue<string>() ?? string.Empty;
                var lang = body["options"]?["raw"]?["language"]?.GetValue<string>()?.ToLowerInvariant();
                tab.BodyType = lang switch
                {
                    "json" => "json",
                    "xml" => "xml",
                    _ => LooksLikeJson(tab.Body) ? "json" : "text"
                };
                break;
            case "urlencoded":
                tab.BodyType = "form";
                if (body["urlencoded"] is JsonArray form)
                    ReadKeyValues(form, tab.FormData);
                break;
            case "formdata":
                tab.BodyType = "multipart";
                if (body["formdata"] is JsonArray fd)
                {
                    foreach (var node in fd)
                    {
                        if (node is not JsonObject obj) continue;
                        var key = obj["key"]?.GetValue<string>() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        var type = obj["type"]?.GetValue<string>() ?? "text";
                        tab.FormData.Add(new Models.KeyValuePair
                        {
                            Key = key,
                            Value = obj["value"]?.GetValue<string>() ?? obj["src"]?.GetValue<string>() ?? string.Empty,
                            Type = type == "file" ? "file" : "text",
                            FilePath = type == "file" ? (obj["src"]?.GetValue<string>() ?? string.Empty) : string.Empty,
                            IsEnabled = obj["disabled"]?.GetValue<bool>() != true
                        });
                    }
                }
                break;
            default:
                tab.BodyType = "none";
                break;
        }
    }

    private static bool LooksLikeJson(string body)
    {
        var t = body.TrimStart();
        return t.StartsWith('{') || t.StartsWith('[');
    }

    private static JsonObject ExportItem(SavedRequest saved)
    {
        var tab = saved.RequestData?.Clone() ?? new RequestTab();
        if (!string.IsNullOrWhiteSpace(saved.Url))
            tab.Url = saved.Url;
        if (!string.IsNullOrWhiteSpace(saved.Method))
            tab.Method = saved.Method;
        if (!string.IsNullOrWhiteSpace(saved.Name))
            tab.Name = saved.Name;

        var request = new JsonObject
        {
            ["method"] = tab.Method,
            ["header"] = ExportKeyValues(tab.Headers, includeType: false),
            ["url"] = ExportUrl(tab)
        };

        var body = ExportBody(tab);
        if (body is not null)
            request["body"] = body;

        var auth = ExportAuth(tab.Auth);
        if (auth is not null)
            request["auth"] = auth;

        return new JsonObject
        {
            ["name"] = saved.Name,
            ["request"] = request
        };
    }

    private static JsonObject ExportUrl(RequestTab tab)
    {
        var raw = tab.Url ?? string.Empty;
        var url = new JsonObject { ["raw"] = raw };

        if (Uri.TryCreate(raw.Split('?')[0], UriKind.Absolute, out var uri))
        {
            url["protocol"] = uri.Scheme;
            url["host"] = new JsonArray(uri.Host.Split('.').Select(s => JsonValue.Create(s)).ToArray());
            url["path"] = new JsonArray(uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => JsonValue.Create(s)).ToArray());
        }

        if (tab.QueryParams.Any(q => q.IsEnabled && !string.IsNullOrWhiteSpace(q.Key)))
            url["query"] = ExportKeyValues(tab.QueryParams, includeType: false);

        return url;
    }

    private static JsonArray ExportKeyValues(IEnumerable<Models.KeyValuePair> pairs, bool includeType)
    {
        var arr = new JsonArray();
        foreach (var p in pairs.Where(p => !string.IsNullOrWhiteSpace(p.Key)))
        {
            var obj = new JsonObject
            {
                ["key"] = p.Key,
                ["value"] = p.Value,
                ["disabled"] = !p.IsEnabled
            };
            if (!string.IsNullOrEmpty(p.Description))
                obj["description"] = p.Description;
            if (includeType)
                obj["type"] = p.Type == "file" ? "file" : "text";
            arr.Add(obj);
        }
        return arr;
    }

    private static JsonObject? ExportBody(RequestTab tab)
    {
        return tab.BodyType switch
        {
            "json" => new JsonObject
            {
                ["mode"] = "raw",
                ["raw"] = tab.Body ?? string.Empty,
                ["options"] = new JsonObject { ["raw"] = new JsonObject { ["language"] = "json" } }
            },
            "xml" => new JsonObject
            {
                ["mode"] = "raw",
                ["raw"] = tab.Body ?? string.Empty,
                ["options"] = new JsonObject { ["raw"] = new JsonObject { ["language"] = "xml" } }
            },
            "text" => new JsonObject
            {
                ["mode"] = "raw",
                ["raw"] = tab.Body ?? string.Empty,
                ["options"] = new JsonObject { ["raw"] = new JsonObject { ["language"] = "text" } }
            },
            "form" => new JsonObject
            {
                ["mode"] = "urlencoded",
                ["urlencoded"] = ExportKeyValues(tab.FormData, includeType: false)
            },
            "multipart" => new JsonObject
            {
                ["mode"] = "formdata",
                ["formdata"] = ExportFormData(tab.FormData)
            },
            _ => null
        };
    }

    private static JsonArray ExportFormData(IEnumerable<Models.KeyValuePair> pairs)
    {
        var arr = new JsonArray();
        foreach (var p in pairs.Where(p => !string.IsNullOrWhiteSpace(p.Key)))
        {
            var obj = new JsonObject
            {
                ["key"] = p.Key,
                ["type"] = p.Type == "file" ? "file" : "text",
                ["disabled"] = !p.IsEnabled
            };
            if (p.Type == "file")
                obj["src"] = p.FilePath;
            else
                obj["value"] = p.Value;
            arr.Add(obj);
        }
        return arr;
    }

    private static JsonObject? ExportAuth(AuthConfig? auth)
    {
        if (auth is null) return null;
        return auth.Type?.ToLowerInvariant() switch
        {
            "bearer" => new JsonObject
            {
                ["type"] = "bearer",
                ["bearer"] = new JsonArray
                {
                    new JsonObject { ["key"] = "token", ["value"] = auth.BearerToken, ["type"] = "string" }
                }
            },
            "basic" => new JsonObject
            {
                ["type"] = "basic",
                ["basic"] = new JsonArray
                {
                    new JsonObject { ["key"] = "username", ["value"] = auth.BasicUsername, ["type"] = "string" },
                    new JsonObject { ["key"] = "password", ["value"] = auth.BasicPassword, ["type"] = "string" }
                }
            },
            "apikey" => new JsonObject
            {
                ["type"] = "apikey",
                ["apikey"] = new JsonArray
                {
                    new JsonObject { ["key"] = "key", ["value"] = auth.ApiKeyName, ["type"] = "string" },
                    new JsonObject { ["key"] = "value", ["value"] = auth.ApiKeyValue, ["type"] = "string" },
                    new JsonObject { ["key"] = "in", ["value"] = auth.ApiKeyLocation, ["type"] = "string" }
                }
            },
            _ => null
        };
    }
}
