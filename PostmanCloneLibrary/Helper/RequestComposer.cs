using PostmanCloneLibrary.Models;

namespace PostmanCloneLibrary.Helper;

public sealed record ComposeResult(RequestTab Request, IReadOnlyList<string> UnresolvedVariables);

public static class RequestComposer
{
    public static ComposeResult Compose(RequestTab source, IReadOnlyDictionary<string, string>? variables)
    {
        var clone = source.Clone();
        NormalizeUrl(clone);
        var unresolved = VariableInterpolator.Apply(clone, variables);
        AuthApplier.Apply(clone);
        return new ComposeResult(clone, unresolved);
    }

    private static void NormalizeUrl(RequestTab request)
    {
        var url = request.Url?.Trim() ?? string.Empty;
        if (url.Length == 0) return;

        if (!url.Contains("://", StringComparison.Ordinal) && !url.StartsWith("{{", StringComparison.Ordinal))
            url = "http://" + url;

        request.Url = url;
    }
}
