using System.Collections.ObjectModel;

namespace PostmanCloneLibrary.Models;

public class RequestTab
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Request";
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public ObservableCollection<KeyValuePair> Headers { get; set; } = [];
    public ObservableCollection<KeyValuePair> QueryParams { get; set; } = [];
    public string Body { get; set; } = string.Empty;
    public string BodyType { get; set; } = "none";
    public ObservableCollection<KeyValuePair> FormData { get; set; } = [];
    public ResponseData? Response { get; set; }
    public bool IsDirty { get; set; }
}
