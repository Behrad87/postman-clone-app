using System.Text.RegularExpressions;

using PostmanCloneLibrary.Models;

namespace PostmanCloneLibrary.Helper;

public static class VariableInterpolator
{
    private static readonly Regex Pattern = new(@"\{\{\s*([^}]+?)\s*\}\}", RegexOptions.Compiled);

    public static string Interpolate(string? input, IReadOnlyDictionary<string, string> variables, ICollection<string> unresolved)
    {
        if (string.IsNullOrEmpty(input) || variables.Count == 0 && !ContainsTokens(input))
            return input ?? string.Empty;

        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Pattern.Replace(input, match =>
        {
            var name = match.Groups[1].Value.Trim();
            if (variables.TryGetValue(name, out var value))
                return value;

            unresolved.Add(name);
            return match.Value;
        });
    }

    public static bool ContainsTokens(string? input)
        => !string.IsNullOrEmpty(input) && input.Contains("{{", StringComparison.Ordinal);

    public static List<string> Apply(RequestTab request, IReadOnlyDictionary<string, string>? variables)
    {
        var unresolved = new List<string>();
        var vars = variables ?? new Dictionary<string, string>();

        request.Url = Interpolate(request.Url, vars, unresolved);
        request.Body = Interpolate(request.Body, vars, unresolved);
        request.Name = Interpolate(request.Name, vars, unresolved);

        foreach (var pair in request.Headers)
        {
            pair.Key = Interpolate(pair.Key, vars, unresolved);
            pair.Value = Interpolate(pair.Value, vars, unresolved);
        }

        foreach (var pair in request.QueryParams)
        {
            pair.Key = Interpolate(pair.Key, vars, unresolved);
            pair.Value = Interpolate(pair.Value, vars, unresolved);
        }

        foreach (var pair in request.FormData)
        {
            pair.Key = Interpolate(pair.Key, vars, unresolved);
            pair.Value = Interpolate(pair.Value, vars, unresolved);
            pair.FilePath = Interpolate(pair.FilePath, vars, unresolved);
        }

        if (request.Auth is not null)
        {
            request.Auth.BearerToken = Interpolate(request.Auth.BearerToken, vars, unresolved);
            request.Auth.BasicUsername = Interpolate(request.Auth.BasicUsername, vars, unresolved);
            request.Auth.BasicPassword = Interpolate(request.Auth.BasicPassword, vars, unresolved);
            request.Auth.ApiKeyName = Interpolate(request.Auth.ApiKeyName, vars, unresolved);
            request.Auth.ApiKeyValue = Interpolate(request.Auth.ApiKeyValue, vars, unresolved);
        }

        return unresolved.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
