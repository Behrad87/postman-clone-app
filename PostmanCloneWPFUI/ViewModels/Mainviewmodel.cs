using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PostmanCloneLibrary;
using PostmanCloneLibrary.ImportExport;
using PostmanCloneLibrary.Models;
using PostmanCloneLibrary.Persistence;
using PostmanCloneWPFUI.Services;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

using AppEnvironment = PostmanCloneLibrary.Models.Environment;

namespace PostmanCloneWPFUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IApiAccess _httpService;
    private readonly IAppStore _store;
    private readonly IDialogService _dialogService;
    private readonly ICollectionView _collectionsView;
    private readonly ICollectionView _historyView;
    private bool _loading;

    [ObservableProperty] private RequestTabViewModel? _activeTab;
    [ObservableProperty] private bool _isSidebarOpen = true;
    [ObservableProperty] private string _sidebarSection = "Collections";
    [ObservableProperty] private CollectionViewModel? _selectedCollection;
    [ObservableProperty] private EnvironmentViewModel? _activeEnvironment;
    [ObservableProperty] private EnvironmentViewModel? _selectedEnvironment;
    [ObservableProperty] private SavedRequestViewModel? _selectedSavedRequest;
    [ObservableProperty] private HistoryItemViewModel? _selectedHistoryItem;

    [ObservableProperty] private string _collectionSearchText = string.Empty;
    [ObservableProperty] private string _historySearchText = string.Empty;

    public ObservableCollection<RequestTabViewModel> Tabs { get; } = [];
    public ObservableCollection<CollectionViewModel> Collections { get; } = [];
    public ObservableCollection<HistoryItemViewModel> History { get; } = [];
    public ObservableCollection<EnvironmentViewModel> Environments { get; } = [];

    public ICollectionView FilteredCollections => _collectionsView;
    public ICollectionView FilteredHistory => _historyView;

    public static readonly string[] HttpMethods = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

    public MainViewModel(IApiAccess httpService, IAppStore store, IDialogService? dialogService = null)
    {
        _httpService = httpService;
        _store = store;
        _dialogService = dialogService ?? new WpfDialogService();

        _collectionsView = CollectionViewSource.GetDefaultView(Collections);
        _collectionsView.Filter = FilterCollection;

        _historyView = CollectionViewSource.GetDefaultView(History);
        _historyView.Filter = FilterHistory;

        Collections.CollectionChanged += OnTreeChanged;
        Environments.CollectionChanged += OnTreeChanged;
        History.CollectionChanged += OnTreeChanged;
        Tabs.CollectionChanged += OnTreeChanged;
    }

    partial void OnCollectionSearchTextChanged(string value) => _collectionsView.Refresh();
    partial void OnHistorySearchTextChanged(string value) => _historyView.Refresh();

    partial void OnActiveTabChanged(RequestTabViewModel? value) => Touch();
    partial void OnActiveEnvironmentChanged(EnvironmentViewModel? value) => Touch();
    partial void OnIsSidebarOpenChanged(bool value) => Touch();
    partial void OnSidebarSectionChanged(string value) => Touch();

    private bool FilterCollection(object obj)
    {
        if (string.IsNullOrWhiteSpace(CollectionSearchText)) return true;
        if (obj is not CollectionViewModel col) return false;

        var term = CollectionSearchText.Trim();
        if (col.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) return true;
        return col.Requests.Any(r =>
            r.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            r.Url.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            r.Method.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private bool FilterHistory(object obj)
    {
        if (string.IsNullOrWhiteSpace(HistorySearchText)) return true;
        if (obj is not HistoryItemViewModel item) return false;

        var term = HistorySearchText.Trim();
        return item.Url.Contains(term, StringComparison.OrdinalIgnoreCase) ||
               item.Method.Contains(term, StringComparison.OrdinalIgnoreCase) ||
               item.StatusCode.ToString().Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    public async Task InitializeAsync()
    {
        _loading = true;
        try
        {
            var state = await _store.LoadAsync();
            ApplyState(state);
            if (Tabs.Count == 0)
                AddNewTab();
        }
        finally
        {
            _loading = false;
            Touch();
        }
    }

    public async Task FlushSaveAsync()
    {
        await _store.SaveNowAsync(CaptureState());
    }

    [RelayCommand]
    public void AddNewTab()
    {
        var tab = CreateTab();
        Tabs.Add(tab);
        ActiveTab = tab;
        Touch();
    }

    [RelayCommand]
    public void DuplicateTab(RequestTabViewModel? tab)
    {
        tab ??= ActiveTab;
        if (tab is null) return;
        var newTab = CreateTab();
        newTab.LoadFrom(tab.ToModel());
        newTab.Name = $"{tab.Name} (Copy)";
        newTab.IsDirty = true;
        var idx = Tabs.IndexOf(tab);
        if (idx >= 0 && idx < Tabs.Count - 1)
            Tabs.Insert(idx + 1, newTab);
        else
            Tabs.Add(newTab);
        ActiveTab = newTab;
        Touch();
    }

    [RelayCommand]
    public void CloseOtherTabs(RequestTabViewModel? tab)
    {
        tab ??= ActiveTab;
        if (tab is null) return;
        var toRemove = Tabs.Where(t => t != tab).ToList();
        foreach (var t in toRemove)
        {
            UnhookTab(t);
            Tabs.Remove(t);
        }
        ActiveTab = tab;
        Touch();
    }

    [RelayCommand]
    private void CloseTab(RequestTabViewModel? tab)
    {
        tab ??= ActiveTab;
        if (tab is null) return;

        if (Tabs.Count == 1)
            AddNewTab();

        var idx = Tabs.IndexOf(tab);
        UnhookTab(tab);
        Tabs.Remove(tab);
        if (Tabs.Count > 0)
            ActiveTab = Tabs[Math.Max(0, Math.Min(idx, Tabs.Count - 1))];
        Touch();
    }

    [RelayCommand]
    private void CloseActiveTab() => CloseTab(ActiveTab);

    [RelayCommand]
    private void SelectTab(RequestTabViewModel tab) => ActiveTab = tab;

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;

    [RelayCommand]
    private void SetSidebarSection(string section) => SidebarSection = section;

    [RelayCommand]
    private void OpenSavedRequest(SavedRequestViewModel? saved)
    {
        if (saved is null) return;
        var tab = CreateTab();
        tab.LoadFrom(saved.RequestData);
        tab.Name = saved.Name;
        tab.IsDirty = false;
        Tabs.Add(tab);
        ActiveTab = tab;
        Touch();
    }

    [RelayCommand]
    private void AddCollection()
    {
        var name = _dialogService.Prompt("New collection", "Collection name:", "New Collection");
        if (name is null) return;
        if (string.IsNullOrWhiteSpace(name)) name = "New Collection";
        var col = new CollectionViewModel { Name = name.Trim() };
        HookCollection(col);
        Collections.Add(col);
        SelectedCollection = col;
        SidebarSection = "Collections";
        Touch();
    }

    [RelayCommand]
    private void RenameCollection(CollectionViewModel? col)
    {
        col ??= SelectedCollection;
        if (col is null) return;
        var name = _dialogService.Prompt("Rename collection", "Collection name:", col.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        col.Name = name.Trim();
        Touch();
    }

    [RelayCommand]
    private void DeleteCollection(CollectionViewModel? col)
    {
        col ??= SelectedCollection;
        if (col is null) return;
        if (!_dialogService.Confirm($"Delete collection “{col.Name}” and its {col.Requests.Count} request(s)?", "Delete collection"))
            return;
        Collections.Remove(col);
        if (SelectedCollection == col) SelectedCollection = null;
        Touch();
    }

    [RelayCommand]
    private void SaveCurrentRequest(CollectionViewModel? col)
    {
        if (ActiveTab is null) return;
        col ??= SelectedCollection ?? Collections.FirstOrDefault();
        if (col is null)
        {
            AddCollection();
            col = SelectedCollection;
            if (col is null) return;
        }

        var saved = SavedRequestViewModel.FromTab(ActiveTab);
        var name = _dialogService.Prompt("Save request", "Request name:", saved.Name);
        if (name is null) return;
        if (!string.IsNullOrWhiteSpace(name))
            saved.Name = name.Trim();
        col.Requests.Add(saved);
        col.IsExpanded = true;
        SelectedCollection = col;
        SidebarSection = "Collections";
        ActiveTab.Name = saved.Name;
        ActiveTab.IsDirty = false;
        Touch();
    }

    [RelayCommand]
    private void DeleteSavedRequest(SavedRequestViewModel? req)
    {
        if (req is null) return;
        var col = Collections.FirstOrDefault(c => c.Requests.Contains(req));
        col?.Requests.Remove(req);
        Touch();
    }

    [RelayCommand]
    private void MoveRequestUp(SavedRequestViewModel? req)
    {
        if (req is null) return;
        var col = Collections.FirstOrDefault(c => c.Requests.Contains(req));
        if (col is null) return;
        var i = col.Requests.IndexOf(req);
        if (i > 0)
        {
            col.Requests.Move(i, i - 1);
            Touch();
        }
    }

    [RelayCommand]
    private void MoveRequestDown(SavedRequestViewModel? req)
    {
        if (req is null) return;
        var col = Collections.FirstOrDefault(c => c.Requests.Contains(req));
        if (col is null) return;
        var i = col.Requests.IndexOf(req);
        if (i >= 0 && i < col.Requests.Count - 1)
        {
            col.Requests.Move(i, i + 1);
            Touch();
        }
    }

    [RelayCommand]
    private void OpenHistoryItem(HistoryItemViewModel? item)
    {
        item ??= SelectedHistoryItem;
        if (item is null) return;
        var tab = CreateTab();
        if (item.Snapshot is not null)
            tab.LoadFrom(item.Snapshot);
        else
        {
            tab.Method = item.Method;
            tab.Url = item.Url;
        }
        tab.Name = $"{item.Method} {item.Url}";
        tab.IsDirty = false;
        Tabs.Add(tab);
        ActiveTab = tab;
        Touch();
    }

    [RelayCommand]
    private void ClearHistory()
    {
        History.Clear();
        Touch();
    }

    [RelayCommand]
    private void AddEnvironment()
    {
        var name = _dialogService.Prompt("New environment", "Environment name:", "Development");
        if (name is null) return;
        if (string.IsNullOrWhiteSpace(name)) name = "Development";
        var env = new EnvironmentViewModel { Name = name.Trim() };
        env.Variables.Add(new EnvironmentVariableViewModel());
        HookEnvironment(env);
        Environments.Add(env);
        ActiveEnvironment = env;
        SelectedEnvironment = env;
        SidebarSection = "Environments";
        Touch();
    }

    [RelayCommand]
    private void DeleteEnvironment(EnvironmentViewModel? env)
    {
        env ??= SelectedEnvironment;
        if (env is null || env.IsGlobals) return;
        if (!_dialogService.Confirm($"Delete environment “{env.Name}”?", "Delete environment"))
            return;
        Environments.Remove(env);
        if (ActiveEnvironment == env)
            ActiveEnvironment = Environments.FirstOrDefault(e => e.IsGlobals) ?? Environments.FirstOrDefault();
        if (SelectedEnvironment == env)
            SelectedEnvironment = ActiveEnvironment;
        Touch();
    }

    [RelayCommand]
    private void RenameEnvironment(EnvironmentViewModel? env)
    {
        env ??= SelectedEnvironment;
        if (env is null || env.IsGlobals) return;
        var name = _dialogService.Prompt("Rename environment", "Environment name:", env.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        env.Name = name.Trim();
        Touch();
    }

    [RelayCommand]
    private void AddEnvironmentVariable()
    {
        SelectedEnvironment ??= Environments.FirstOrDefault(e => e.IsGlobals);
        SelectedEnvironment?.Variables.Add(new EnvironmentVariableViewModel());
        Touch();
    }

    [RelayCommand]
    private void RemoveEnvironmentVariable(EnvironmentVariableViewModel? item)
    {
        if (item is null || SelectedEnvironment is null) return;
        SelectedEnvironment.Variables.Remove(item);
        Touch();
    }

    [RelayCommand]
    private void SelectEnvironmentForEdit(EnvironmentViewModel env)
    {
        SelectedEnvironment = env;
        if (!env.IsGlobals)
            ActiveEnvironment = env;
    }

    [RelayCommand]
    private void ImportCollection()
    {
        var path = _dialogService.ShowOpenFileDialog("Import Postman collection", "Postman Collection (*.json)|*.json|All files (*.*)|*.*");
        if (path is null) return;

        try
        {
            var json = File.ReadAllText(path);
            var model = PostmanCollectionConverter.Import(json);
            var vm = CollectionViewModel.FromModel(model);
            HookCollection(vm);
            Collections.Add(vm);
            SelectedCollection = vm;
            SidebarSection = "Collections";
            Touch();
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage("Could not import that file as a Postman v2.1 collection.\n\n" + ex.Message, "Import failed");
        }
    }

    [RelayCommand]
    private void ExportCollection(CollectionViewModel? col)
    {
        col ??= SelectedCollection;
        if (col is null) return;
        var defaultName = SanitizeFileName(col.Name) + ".postman_collection.json";
        var path = _dialogService.ShowSaveFileDialog("Export collection", defaultName, "Postman Collection (*.json)|*.json");
        if (path is null) return;

        try
        {
            File.WriteAllText(path, PostmanCollectionConverter.Export(col.ToModel()));
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage("Could not export the collection.\n\n" + ex.Message, "Export failed");
        }
    }

    private void ApplyState(AppState state)
    {
        Collections.Clear();
        foreach (var c in state.Collections)
        {
            var vm = CollectionViewModel.FromModel(c);
            HookCollection(vm);
            Collections.Add(vm);
        }

        if (Collections.Count == 0)
            SeedSampleCollection();

        Environments.Clear();
        foreach (var e in state.Environments)
        {
            var vm = EnvironmentViewModel.FromModel(e);
            HookEnvironment(vm);
            Environments.Add(vm);
        }

        if (!Environments.Any(e => e.IsGlobals))
        {
            var globals = new EnvironmentViewModel
            {
                Id = AppEnvironment.GlobalsId,
                Name = "Globals",
                IsGlobals = true
            };
            HookEnvironment(globals);
            Environments.Insert(0, globals);
        }

        if (Environments.Count == 1)
        {
            var dev = new EnvironmentViewModel { Name = "Development" };
            HookEnvironment(dev);
            Environments.Add(dev);
        }

        History.Clear();
        foreach (var h in state.History)
            History.Add(HistoryItemViewModel.FromModel(h));

        var session = state.Session ?? new SessionState();
        IsSidebarOpen = session.IsSidebarOpen;
        if (!string.IsNullOrWhiteSpace(session.SidebarSection))
            SidebarSection = session.SidebarSection;

        ActiveEnvironment = Environments.FirstOrDefault(e => e.Id == session.ActiveEnvironmentId)
            ?? Environments.FirstOrDefault(e => !e.IsGlobals)
            ?? Environments.FirstOrDefault();
        SelectedEnvironment = ActiveEnvironment;

        foreach (var tabModel in session.OpenTabs)
        {
            var tab = CreateTab();
            tab.LoadFrom(tabModel);
            Tabs.Add(tab);
        }

        if (Tabs.Count > 0)
        {
            ActiveTab = Tabs.FirstOrDefault(t => t.Id == session.ActiveTabId) ?? Tabs[0];
        }
    }

    private AppState CaptureState()
    {
        return new AppState
        {
            Collections = Collections.Select(c => c.ToModel()).ToList(),
            Environments = Environments.Select(e => e.ToModel()).ToList(),
            History = History.Select(h => h.ToModel()).ToList(),
            Session = new SessionState
            {
                OpenTabs = Tabs.Select(t => t.ToModel()).ToList(),
                ActiveTabId = ActiveTab?.Id,
                ActiveEnvironmentId = ActiveEnvironment?.Id,
                SidebarSection = SidebarSection,
                IsSidebarOpen = IsSidebarOpen
            }
        };
    }

    private void Touch()
    {
        if (_loading) return;
        _store.ScheduleSave(CaptureState());
    }

    private RequestTabViewModel CreateTab()
    {
        var tab = new RequestTabViewModel(_httpService, _dialogService)
        {
            ResolveVariables = BuildVariableMap,
            ResolveEnvironmentDisableSsl = () =>
                (ActiveEnvironment?.DisableSslVerification ?? false)
                || (Environments.FirstOrDefault(e => e.IsGlobals)?.DisableSslVerification ?? false),
            Changed = Touch,
            Completed = OnRequestCompleted
        };
        return tab;
    }

    private void UnhookTab(RequestTabViewModel tab)
    {
        tab.Changed = null;
        tab.Completed = null;
        tab.ResolveVariables = null;
        tab.ResolveEnvironmentDisableSsl = null;
    }

    private IReadOnlyDictionary<string, string> BuildVariableMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var globals = Environments.FirstOrDefault(e => e.IsGlobals);
        if (globals is not null)
        {
            foreach (var v in globals.Variables.Where(v => !string.IsNullOrWhiteSpace(v.Key)))
                map[v.Key] = v.Value ?? string.Empty;
        }

        if (ActiveEnvironment is not null && !ActiveEnvironment.IsGlobals)
        {
            foreach (var v in ActiveEnvironment.Variables.Where(v => !string.IsNullOrWhiteSpace(v.Key)))
                map[v.Key] = v.Value ?? string.Empty;
        }

        return map;
    }

    private void OnRequestCompleted(RequestTabViewModel tab, ResponseData response)
    {
        var item = new HistoryItemViewModel
        {
            Method = tab.Method,
            Url = tab.Url,
            StatusCode = response.StatusCode,
            Timestamp = DateTime.Now,
            ElapsedMs = response.ElapsedMs,
            Snapshot = tab.ToModel()
        };
        History.Insert(0, item);
        while (History.Count > JsonAppStore.HistoryCap)
            History.RemoveAt(History.Count - 1);
        Touch();
    }

    private void HookCollection(CollectionViewModel col)
    {
        col.PropertyChanged += (_, _) => Touch();
        col.Requests.CollectionChanged += OnTreeChanged;
    }

    private void HookEnvironment(EnvironmentViewModel env)
    {
        env.PropertyChanged += (_, _) => Touch();
        env.Variables.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (EnvironmentVariableViewModel v in e.NewItems)
                    v.PropertyChanged += (_, _) => Touch();
            }
            Touch();
        };
        foreach (var v in env.Variables)
            v.PropertyChanged += (_, _) => Touch();
    }

    private void OnTreeChanged(object? sender, NotifyCollectionChangedEventArgs e) => Touch();

    private void SeedSampleCollection()
    {
        var col = new CollectionViewModel { Name = "Sample Requests" };
        col.Requests.Add(SavedRequestViewModel.FromModel(new SavedRequest
        {
            Name = "JSONPlaceholder Posts",
            Method = "GET",
            Url = "https://jsonplaceholder.typicode.com/posts"
        }));
        col.Requests.Add(SavedRequestViewModel.FromModel(new SavedRequest
        {
            Name = "JSONPlaceholder Users",
            Method = "GET",
            Url = "https://jsonplaceholder.typicode.com/users"
        }));
        col.Requests.Add(SavedRequestViewModel.FromModel(new SavedRequest
        {
            Name = "HTTPBin GET",
            Method = "GET",
            Url = "https://httpbin.org/get"
        }));
        HookCollection(col);
        Collections.Add(col);
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "collection" : name;
    }
}
