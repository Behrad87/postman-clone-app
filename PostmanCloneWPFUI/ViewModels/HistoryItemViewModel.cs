using CommunityToolkit.Mvvm.ComponentModel;

using PostmanCloneLibrary.Models;

namespace PostmanCloneWPFUI.ViewModels;

public partial class HistoryItemViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string _method = "GET";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private int _statusCode;
    [ObservableProperty] private DateTime _timestamp = DateTime.Now;
    [ObservableProperty] private long _elapsedMs;

    public RequestTab? Snapshot { get; set; }

    public HistoryItem ToModel() => new()
    {
        Id = Id,
        Method = Method,
        Url = Url,
        StatusCode = StatusCode,
        Timestamp = Timestamp,
        ElapsedMs = ElapsedMs,
        RequestSnapshot = Snapshot?.Clone()
    };

    public static HistoryItemViewModel FromModel(HistoryItem model) => new()
    {
        Id = model.Id,
        Method = model.Method,
        Url = model.Url,
        StatusCode = model.StatusCode,
        Timestamp = model.Timestamp,
        ElapsedMs = model.ElapsedMs,
        Snapshot = model.RequestSnapshot?.Clone()
    };
}
