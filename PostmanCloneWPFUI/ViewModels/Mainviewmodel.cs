using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PostmanCloneLibrary;

using System.Collections.ObjectModel;

namespace PostmanCloneWPFUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IApiAccess _httpService;

    [ObservableProperty] private RequestTabViewModel? _activeTab;
    [ObservableProperty] private bool _isSidebarOpen = true;
    [ObservableProperty] private string _sidebarSection = "Collections"; // Collections, Environments, History

    public ObservableCollection<RequestTabViewModel> Tabs { get; } = new();
    public ObservableCollection<CollectionViewModel> Collections { get; } = new();
    public ObservableCollection<HistoryItemViewModel> History { get; } = new();

    public static readonly string[] HttpMethods = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

    public MainViewModel(IApiAccess httpService)
    {
        _httpService = httpService;
        AddNewTab(); // start with one tab
        SeedCollections();
    }

    [RelayCommand]
    public void AddNewTab()
    {
        var tab = new RequestTabViewModel(_httpService);
        Tabs.Add(tab);
        ActiveTab = tab;
    }

    [RelayCommand]
    private void CloseTab(RequestTabViewModel tab)
    {
        if (Tabs.Count == 1) { AddNewTab(); }
        var idx = Tabs.IndexOf(tab);
        Tabs.Remove(tab);
        ActiveTab = Tabs[Math.Max(0, Math.Min(idx, Tabs.Count - 1))];
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
    }

    [RelayCommand]
    private void SetSidebarSection(string section)
    {
        SidebarSection = section;
    }

    [RelayCommand]
    private void OpenSavedRequest(SavedRequestViewModel saved)
    {
        var tab = new RequestTabViewModel(_httpService)
        {
            Name = saved.Name,
            Method = saved.Method,
            Url = saved.Url
        };
        Tabs.Add(tab);
        ActiveTab = tab;
    }

    [RelayCommand]
    private void AddCollection()
    {
        Collections.Add(new CollectionViewModel { Name = "New Collection" });
    }

    private void SeedCollections()
    {
        var col = new CollectionViewModel { Name = "Sample Requests" };
        col.Requests.Add(new SavedRequestViewModel { Name = "JSONPlaceholder Posts", Method = "GET", Url = "https://jsonplaceholder.typicode.com/posts" });
        col.Requests.Add(new SavedRequestViewModel { Name = "JSONPlaceholder Users", Method = "GET", Url = "https://jsonplaceholder.typicode.com/users" });
        col.Requests.Add(new SavedRequestViewModel { Name = "Create Post", Method = "POST", Url = "https://jsonplaceholder.typicode.com/posts" });
        col.Requests.Add(new SavedRequestViewModel { Name = "HTTPBin GET", Method = "GET", Url = "https://httpbin.org/get" });
        col.Requests.Add(new SavedRequestViewModel { Name = "HTTPBin POST", Method = "POST", Url = "https://httpbin.org/post" });
        Collections.Add(col);
    }
}

public partial class CollectionViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "Collection";
    [ObservableProperty] private bool _isExpanded = true;
    public ObservableCollection<SavedRequestViewModel> Requests { get; } = new();
}

public partial class SavedRequestViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _method = "GET";
    [ObservableProperty] private string _url = string.Empty;
}

public partial class HistoryItemViewModel : ObservableObject
{
    [ObservableProperty] private string _method = "GET";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private int _statusCode;
    [ObservableProperty] private DateTime _timestamp;
}