using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PostmanCloneLibrary;
using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;

using System.Collections.ObjectModel;
using System.Security.Policy;

namespace PostmanCloneWPFUI.ViewModels;
public partial class RequestTabViewModel : ObservableObject
{
    private readonly IApiAccess _httpService;
    private CancellationTokenSource? _cts;

    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty] private string _name = "New Request";
    [ObservableProperty] private string _method = "GET";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private bool _isSending;
    [ObservableProperty] private bool _hasResponse;
    [ObservableProperty] private bool _isDirty;

    // Body
    [ObservableProperty] private string _bodyType = "none"; // none, json, text, xml, form
    [ObservableProperty] private string _bodyContent = string.Empty;

    // Request sub-tab
    [ObservableProperty] private string _activeRequestTab = "Params";

    // Response
    [ObservableProperty] private int _responseStatus;
    [ObservableProperty] private string _responseStatusText = string.Empty;
    [ObservableProperty] private long _responseElapsedMs;
    [ObservableProperty] private string _responseSize = string.Empty;
    [ObservableProperty] private string _responseBody = string.Empty;
    [ObservableProperty] private string _responseContentType = string.Empty;
    [ObservableProperty] private bool _responseIsSuccess;
    [ObservableProperty] private string _activeResponseTab = "Body";

    public ObservableCollection<KeyValueItemViewModel> Headers { get; } = new();
    public ObservableCollection<KeyValueItemViewModel> QueryParams { get; } = new();
    public ObservableCollection<KeyValueItemViewModel> FormData { get; } = new();
    public ObservableCollection<ResponseHeaderItemViewModel> ResponseHeaders { get; } = new();

    public static readonly string[] Methods = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

    public RequestTabViewModel(IApiAccess httpService)
    {
        _httpService = httpService;
        AddEmptyHeader();
        AddEmptyParam();
    }

    partial void OnUrlChanged(string value) => IsDirty = true;
    partial void OnMethodChanged(string value) => IsDirty = true;
    partial void OnBodyContentChanged(string value) => IsDirty = true;

    [RelayCommand]
    private void AddHeader()
    {
        Headers.Add(new KeyValueItemViewModel());
    }

    [RelayCommand]
    private void RemoveHeader(KeyValueItemViewModel item)
    {
        Headers.Remove(item);
    }

    [RelayCommand]
    private void AddParam()
    {
        QueryParams.Add(new KeyValueItemViewModel());
        RebuildUrlQuery();
    }

    [RelayCommand]
    private void RemoveParam(KeyValueItemViewModel item)
    {
        QueryParams.Remove(item);
        RebuildUrlQuery();
    }

    [RelayCommand]
    private void AddFormField()
    {
        FormData.Add(new KeyValueItemViewModel());
    }

    [RelayCommand]
    private void RemoveFormField(KeyValueItemViewModel item)
    {
        FormData.Remove(item);
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        IsSending = true;
        HasResponse = false;

        try
        {
            var reqModel = BuildRequestModel();
            var response = await _httpService.SendAsync(reqModel, _cts.Token);
            ApplyResponse(response);
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private void CancelRequest()
    {
        _cts?.Cancel();
        IsSending = false;
    }

    [RelayCommand]
    private void ClearResponse()
    {
        HasResponse = false;
        ResponseBody = string.Empty;
        ResponseHeaders.Clear();
    }

    [RelayCommand]
    private void CopyResponseBody()
    {
        System.Windows.Clipboard.SetText(ResponseBody);
    }

    private RequestTab BuildRequestModel()
    {
        return new RequestTab
        {
            Method = Method,
            Url = Url,
            Headers = new System.Collections.ObjectModel.ObservableCollection<PostmanCloneLibrary.Models.KeyValuePair>(
                Headers.Select(h => h.ToModel())),
            QueryParams = new System.Collections.ObjectModel.ObservableCollection<PostmanCloneLibrary.Models.KeyValuePair>(
                QueryParams.Select(p => p.ToModel())),
            BodyType = BodyType,
            Body = BodyContent,
            FormData = new System.Collections.ObjectModel.ObservableCollection<PostmanCloneLibrary.Models.KeyValuePair>(
                FormData.Select(f => f.ToModel()))
        };
    }

    private void ApplyResponse(ResponseData resp)
    {
        ResponseStatus = resp.StatusCode;
        ResponseStatusText = resp.StatusText;
        ResponseElapsedMs = resp.ElapsedMs;
        ResponseSize = JsonFormatter.FormatSize(resp.SizeBytes);
        ResponseIsSuccess = resp.IsSuccess;
        ResponseContentType = resp.ContentType;

        ResponseBody = JsonFormatter.IsJson(resp.ContentType)
            ? JsonFormatter.TryFormat(resp.Body)
            : resp.Body;

        ResponseHeaders.Clear();
        foreach (var h in resp.Headers)
            ResponseHeaders.Add(new ResponseHeaderItemViewModel { Key = h.Key, Value = h.Value });

        HasResponse = true;
        ActiveResponseTab = "Body";
    }

    private void AddEmptyHeader()
    {
        Headers.Add(new KeyValueItemViewModel { Key = "Content-Type", Value = "application/json" });
        Headers.Add(new KeyValueItemViewModel());
    }

    private void AddEmptyParam()
    {
        QueryParams.Add(new KeyValueItemViewModel());
    }

    private void RebuildUrlQuery() { /* optional: sync URL bar with params */ }
}

public class ResponseHeaderItemViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}