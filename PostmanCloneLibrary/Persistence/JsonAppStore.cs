using System.Text.Json;
using System.Text.Json.Serialization;

using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;

using AppEnvironment = PostmanCloneLibrary.Models.Environment;

namespace PostmanCloneLibrary.Persistence;

public sealed class JsonAppStore : IAppStore
{
    public const int HistoryCap = 200;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _dir;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CancellationTokenSource? _debounce;
    private string? _pendingCollections;
    private string? _pendingEnvironments;
    private string? _pendingHistory;
    private string? _pendingSession;

    public JsonAppStore(string? directory = null)
    {
        _dir = directory ?? Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostmanCloneApp");
        Directory.CreateDirectory(_dir);
    }

    public async Task<AppState> LoadAsync()
    {
        var state = new AppState
        {
            Collections = await ReadListAsync<Collection>("collections.json") ?? [],
            Environments = await ReadListAsync<AppEnvironment>("environments.json") ?? [],
            History = await ReadListAsync<HistoryItem>("history.json") ?? [],
            Session = await ReadAsync<SessionState>("session.json") ?? new SessionState()
        };

        UnprotectSecrets(state.Environments);

        if (!state.Environments.Any(e => e.IsGlobals))
        {
            state.Environments.Insert(0, new AppEnvironment
            {
                Id = AppEnvironment.GlobalsId,
                Name = "Globals",
                IsGlobals = true
            });
        }

        if (state.History.Count > HistoryCap)
            state.History = state.History.Take(HistoryCap).ToList();

        return state;
    }

    public void ScheduleSave(AppState state)
    {
        Snapshot(state);
        _debounce?.Cancel();
        _debounce?.Dispose();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;
        _ = DebounceWriteAsync(token);
    }

    public async Task SaveNowAsync(AppState state)
    {
        Snapshot(state);
        await WritePendingAsync();
    }

    private void Snapshot(AppState state)
    {
        var collections = state.Collections ?? [];
        var environments = CloneEnvironments(state.Environments ?? []);
        ProtectSecrets(environments);
        var history = (state.History ?? []).Take(HistoryCap).ToList();
        foreach (var item in history)
        {
            if (item.RequestSnapshot is not null)
                item.RequestSnapshot.Response = null;
        }

        _pendingCollections = JsonSerializer.Serialize(collections, Options);
        _pendingEnvironments = JsonSerializer.Serialize(environments, Options);
        _pendingHistory = JsonSerializer.Serialize(history, Options);
        _pendingSession = JsonSerializer.Serialize(state.Session ?? new SessionState(), Options);
    }

    private async Task DebounceWriteAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(500, token);
            await WritePendingAsync();
        }
        catch (TaskCanceledException)
        {
            // newer save scheduled
        }
    }

    private async Task WritePendingAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_pendingCollections is not null)
                await File.WriteAllTextAsync(Path.Combine(_dir, "collections.json"), _pendingCollections);
            if (_pendingEnvironments is not null)
                await File.WriteAllTextAsync(Path.Combine(_dir, "environments.json"), _pendingEnvironments);
            if (_pendingHistory is not null)
                await File.WriteAllTextAsync(Path.Combine(_dir, "history.json"), _pendingHistory);
            if (_pendingSession is not null)
                await File.WriteAllTextAsync(Path.Combine(_dir, "session.json"), _pendingSession);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<T>?> ReadListAsync<T>(string fileName)
    {
        var path = Path.Combine(_dir, fileName);
        if (!File.Exists(path)) return null;
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, Options);
        }
        catch
        {
            return null;
        }
    }

    private async Task<T?> ReadAsync<T>(string fileName) where T : class
    {
        var path = Path.Combine(_dir, fileName);
        if (!File.Exists(path)) return null;
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, Options);
        }
        catch
        {
            return null;
        }
    }

    private static List<AppEnvironment> CloneEnvironments(IEnumerable<AppEnvironment> source)
    {
        var json = JsonSerializer.Serialize(source, Options);
        return JsonSerializer.Deserialize<List<AppEnvironment>>(json, Options) ?? [];
    }

    private static void ProtectSecrets(IEnumerable<AppEnvironment> environments)
    {
        foreach (var env in environments)
        {
            foreach (var v in env.Variables.Where(v => v.IsSecret))
                v.Value = SecretProtector.Protect(v.Value);
        }
    }

    private static void UnprotectSecrets(IEnumerable<AppEnvironment> environments)
    {
        foreach (var env in environments)
        {
            foreach (var v in env.Variables.Where(v => v.IsSecret))
                v.Value = SecretProtector.Unprotect(v.Value);
        }
    }
}
