using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace PostmanCloneLibrary.Helper;

public static class SecretProtector
{
    private const string Prefix = "dpapi:";
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("PostmanCloneApp.v1");

    public static string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext) || plaintext.StartsWith(Prefix, StringComparison.Ordinal))
            return plaintext;

        if (!OperatingSystem.IsWindows())
            return plaintext;

        try
        {
            var bytes = ProtectWindows(plaintext);
            return Prefix + Convert.ToBase64String(bytes);
        }
        catch
        {
            return plaintext;
        }
    }

    public static string Unprotect(string stored)
    {
        if (string.IsNullOrEmpty(stored) || !stored.StartsWith(Prefix, StringComparison.Ordinal))
            return stored;

        if (!OperatingSystem.IsWindows())
            return stored;

        try
        {
            var bytes = Convert.FromBase64String(stored[Prefix.Length..]);
            return UnprotectWindows(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    [SupportedOSPlatform("windows")]
    private static byte[] ProtectWindows(string plaintext)
        => ProtectedData.Protect(Encoding.UTF8.GetBytes(plaintext), Entropy, DataProtectionScope.CurrentUser);

    [SupportedOSPlatform("windows")]
    private static string UnprotectWindows(byte[] bytes)
        => Encoding.UTF8.GetString(ProtectedData.Unprotect(bytes, Entropy, DataProtectionScope.CurrentUser));
}
