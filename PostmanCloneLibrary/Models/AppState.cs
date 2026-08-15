namespace PostmanCloneLibrary.Models;

public class AppState
{
    public List<Collection> Collections { get; set; } = [];
    public List<Environment> Environments { get; set; } = [];
    public List<HistoryItem> History { get; set; } = [];
    public SessionState Session { get; set; } = new();
}

public class SessionState
{
    public List<RequestTab> OpenTabs { get; set; } = [];
    public Guid? ActiveTabId { get; set; }
    public Guid? ActiveEnvironmentId { get; set; }
    public string SidebarSection { get; set; } = "Collections";
    public bool IsSidebarOpen { get; set; } = true;
}
