using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;
using PostmanCloneLibrary.Persistence;
using Xunit;

namespace PostmanCloneLibrary.Tests;

public class SecretProtectorAndStoreTests
{
    [Fact]
    public void SecretProtector_ProtectsAndUnprotects_OnWindows()
    {
        var secret = "super-secret-api-key-12345!";
        var protectedValue = SecretProtector.Protect(secret);

        if (OperatingSystem.IsWindows())
        {
            Assert.StartsWith("dpapi:", protectedValue);
            var recovered = SecretProtector.Unprotect(protectedValue);
            Assert.Equal(secret, recovered);
        }
        else
        {
            Assert.Equal(secret, protectedValue);
        }
    }

    [Fact]
    public async Task JsonAppStore_CreatesDefaultGlobalsEnvironment()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "postman_clone_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonAppStore(tempDir);
            var state = await store.LoadAsync();

            Assert.NotNull(state.Environments);
            var globals = Assert.Single(state.Environments, e => e.IsGlobals);
            Assert.Equal("Globals", globals.Name);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }
}
