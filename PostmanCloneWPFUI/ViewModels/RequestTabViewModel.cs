using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PostmanCloneLibrary;
using PostmanCloneLibrary.Helper;
using PostmanCloneLibrary.Models;
using PostmanCloneWPFUI.Services;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PostmanCloneWPFUI.ViewModels;

public partial class RequestTabViewModel : ObservableObject
{
    private readonly IApiAccess _httpService;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cts;
    private bool _isSyncingUrlAndParams;

    public Guid Id { get; set; } = Guid.NewGuid();

    public Func<IReadOnlyDictionary<string, string>>? ResolveVariables { get; set; }
    public Func<bool>? ResolveEnvironmentDisableSsl { get; set; }
    public Action? Changed { get; set; }
    public Action<RequestTabViewModel, ResponseData>? Completed { get; set; }

    [ObservableProperty] private string _name = "New Request";
    [ObservableProperty] private string _method = "GET";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private bool _isSending;
    [ObservableProperty] private bool _hasResponse;
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private bool _disableSslVerification;

    [ObservableProperty] private string _bodyType = "none";
    [ObservableProperty] private string _bodyContent = string.Empty;

    [ObservableProperty] private string _authType = "none";
    [ObservableProperty] private string _bearerToken = string.Empty;
    [ObservableProperty] private string _basicUsername = string.Empty;
    [ObservableProperty] private string _basicPassword = string.Empty;
    [ObservableProperty] private string _apiKeyName = "X-API-Key";
    [ObservableProperty] private string _apiKeyValue = string.Empty;
    [ObservableProperty] private string _apiKeyLocation = "header";

    [ObservableProperty] private string _activeRequestTab = "Params";

    [ObservableProperty] private int _responseStatus;
    [ObservableProperty] private string _responseStatusText = string.Empty;
    [ObservableProperty] private long _responseElapsedMs;
    [ObservableProperty] private string _responseSize = string.Empty;
    [ObservableProperty] private string _rawResponseBody = string.Empty;
    [ObservableProperty] private string _prettyResponseBody = string.Empty;
    [ObservableProperty] private string _responseContentType = string.Empty;
    [ObservableProperty] private bool _responseIsSuccess;
    [ObservableProperty] private string _activeResponseTab = "Body";
    [ObservableProperty] private bool _isPrettyResponse = true;
    [ObservableProperty] private bool _isTransportError;
    [ObservableProperty] private string _transportErrorMessage = string.Empty;
    [ObservableProperty] private string _unresolvedWarning = string.Empty;

    [ObservableProperty] private string _copyButtonText = "Copy";
    [ObservableProperty] private string _responseSearchText = string.Empty;

    public ObservableCollection<KeyValueItemViewModel> Headers { get; } = [];
    public ObservableCollection<KeyValueItemViewModel> QueryParams { get; } = [];
    public ObservableCollection<KeyValueItemViewModel> FormData { get; } = [];
    public ObservableCollection<ResponseHeaderItemViewModel> ResponseHeaders { get; } = [];

    public static readonly string[] Methods = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];
    public string[] HttpMethods => Methods;

    public string DisplayedResponseBody => IsPrettyResponse ? PrettyResponseBody : RawResponseBody;
    public bool ShouldHighlightJson =>
        !IsTransportError && JsonFormatter.LooksLikeJson(DisplayedResponseBody);
    public bool HasUnresolvedWarning => !string.IsNullOrWhiteSpace(UnresolvedWarning);

    public string ParamsBadge
    {
        get
        {
            var count = QueryParams.Count(p => p.IsEnabled && !string.IsNullOrWhiteSpace(p.Key));
            return count > 0 ? $" ({count})" : string.Empty;
        }
    }

    public string HeadersBadge
    {
        get
        {
            var count = Headers.Count(h => h.IsEnabled && !string.IsNullOrWhiteSpace(h.Key));
            return count > 0 ? $" ({count})" : string.Empty;
        }
    }

    public string BodyBadge => BodyType is not "none" ? $" ({BodyType.ToUpperInvariant()})" : string.Empty;
    public string AuthBadge => AuthType is not "none" ? $" ({AuthType.ToUpperInvariant()})" : string.Empty;

    public RequestTabViewModel(IApiAccess httpService, IDialogService? dialogService = null)
    {
        _httpService = httpService;
        _dialogService = dialogService ?? new WpfDialogService();
        Track(Headers);
        Track(QueryParams);
        Track(FormData);
        AddEmptyHeader();
        AddEmptyParam();
        IsDirty = false;
    }

    partial void OnUrlChanged(string value)
    {
        MarkDirty();
        if (!_isSyncingUrlAndParams)
            SyncUrlToQueryParams(value);
    }

    partial void OnMethodChanged(string value) => MarkDirty();
    partial void OnBodyContentChanged(string value) => MarkDirty();
    partial void OnNameChanged(string value) => MarkDirty();
    partial void OnBodyTypeChanged(string value)
    {
        MarkDirty();
        NotifyBadges();
        if ((value is "form" or "multipart") && FormData.Count == 0)
            FormData.Add(new KeyValueItemViewModel());
    }
    partial void OnAuthTypeChanged(string value)
    {
        MarkDirty();
        NotifyBadges();
    }
    partial void OnBearerTokenChanged(string value) => MarkDirty();
    partial void OnBasicUsernameChanged(string value) => MarkDirty();
    partial void OnBasicPasswordChanged(string value) => MarkDirty();
    partial void OnApiKeyNameChanged(string value) => MarkDirty();
    partial void OnApiKeyValueChanged(string value) => MarkDirty();
    partial void OnApiKeyLocationChanged(string value) => MarkDirty();
    partial void OnDisableSslVerificationChanged(bool value) => MarkDirty();
    partial void OnUnresolvedWarningChanged(string value)
        => OnPropertyChanged(nameof(HasUnresolvedWarning));

    partial void OnIsPrettyResponseChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayedResponseBody));
        OnPropertyChanged(nameof(ShouldHighlightJson));
    }

    public void NotifyBadges()
    {
        OnPropertyChanged(nameof(ParamsBadge));
        OnPropertyChanged(nameof(HeadersBadge));
        OnPropertyChanged(nameof(BodyBadge));
        OnPropertyChanged(nameof(AuthBadge));
    }

    private void SyncUrlToQueryParams(string url)
    {
        if (_isSyncingUrlAndParams) return;
        _isSyncingUrlAndParams = true;
        try
        {
            var qIndex = url.IndexOf('?');
            if (qIndex < 0 || qIndex == url.Length - 1)
            {
                return;
            }

            var queryString = url[(qIndex + 1)..];
            var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in QueryParams)
                p.PropertyChanged -= ItemChanged;
            QueryParams.Clear();

            foreach (var pair in pairs)
            {
                var eqIndex = pair.IndexOf('=');
                string k, v;
                if (eqIndex >= 0)
                {
                    k = Uri.UnescapeDataString(pair[..eqIndex]);
                    v = Uri.UnescapeDataString(pair[(eqIndex + 1)..]);
                }
                else
                {
                    k = Uri.UnescapeDataString(pair);
                    v = string.Empty;
                }
                var item = new KeyValueItemViewModel { Key = k, Value = v, IsEnabled = true };
                item.PropertyChanged += ItemChanged;
                QueryParams.Add(item);
            }
            AddEmptyParam();
            NotifyBadges();
        }
        catch
        {
            // ignore malformed URI encoding issues during typing
        }
        finally
        {
            _isSyncingUrlAndParams = false;
        }
    }

    private void SyncQueryParamsToUrl()
    {
        if (_isSyncingUrlAndParams) return;
        _isSyncingUrlAndParams = true;
        try
        {
            var currentUrl = Url ?? string.Empty;
            var qIndex = currentUrl.IndexOf('?');
            var baseUrl = qIndex >= 0 ? currentUrl[..qIndex] : currentUrl;

            var active = QueryParams.Where(q => q.IsEnabled && !string.IsNullOrWhiteSpace(q.Key)).ToList();
            if (active.Count == 0)
            {
                if (qIndex >= 0)
                    Url = baseUrl;
            }
            else
            {
                var qs = string.Join("&", active.Select(q =>
                    $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value ?? string.Empty)}"));
                Url = baseUrl + "?" + qs;
            }
        }
        catch
        {
        }
        finally
        {
            _isSyncingUrlAndParams = false;
        }
    }

    [RelayCommand]
    private void SetRequestTab(string tab) => ActiveRequestTab = tab;

    [RelayCommand]
    private void SetResponseTab(string tab) => ActiveResponseTab = tab;

    [RelayCommand]
    private void AddHeader() => Headers.Add(new KeyValueItemViewModel());

    [RelayCommand]
    private void RemoveHeader(KeyValueItemViewModel item) => Headers.Remove(item);

    [RelayCommand]
    private void AddParam() => QueryParams.Add(new KeyValueItemViewModel());

    [RelayCommand]
    private void RemoveParam(KeyValueItemViewModel item) => QueryParams.Remove(item);

    [RelayCommand]
    private void AddFormField() => FormData.Add(new KeyValueItemViewModel());

    [RelayCommand]
    private void RemoveFormField(KeyValueItemViewModel item) => FormData.Remove(item);

    [RelayCommand]
    private void BrowseFormFile(KeyValueItemViewModel item)
    {
        var path = _dialogService.ShowOpenFileDialog("Select file to upload");
        if (path is null) return;
        item.ItemType = "file";
        item.FilePath = path;
        item.Value = System.IO.Path.GetFileName(path);
    }

    [RelayCommand]
    private void ToggleFormFieldType(KeyValueItemViewModel item)
    {
        item.ItemType = item.ItemType == "file" ? "text" : "file";
        if (item.ItemType == "text")
            item.FilePath = string.Empty;
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        IsSending = true;
        HasResponse = false;
        UnresolvedWarning = string.Empty;

        try
        {
            var variables = ResolveVariables?.Invoke() ?? new Dictionary<string, string>();
            var model = ToModel();
            model.DisableSslVerification = DisableSslVerification || (ResolveEnvironmentDisableSsl?.Invoke() ?? false);

            var composed = RequestComposer.Compose(model, variables);
            if (composed.UnresolvedVariables.Count > 0)
                UnresolvedWarning = "Unresolved variables: " + string.Join(", ", composed.UnresolvedVariables.Select(v => "{{" + v + "}}"));

            if (string.IsNullOrWhiteSpace(composed.Request.Url))
            {
                ApplyTransportError("Invalid URL", "Enter a URL before sending.");
                return;
            }

            if (!_httpService.IsValidUrl(composed.Request.Url))
            {
                ApplyTransportError("Invalid URL", $"Not a valid http/https URL: {composed.Request.Url}");
                return;
            }

            var response = await _httpService.SendAsync(composed.Request, _cts.Token);
            ApplyResponse(response);
            Completed?.Invoke(this, response);
        }
        catch (OperationCanceledException)
        {
            ApplyTransportError("Cancelled", "Request was cancelled.");
        }
        catch (Exception ex)
        {
            ApplyTransportError("Error", $"An unexpected error occurred: {ex.Message}");
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
        RawResponseBody = string.Empty;
        PrettyResponseBody = string.Empty;
        ResponseHeaders.Clear();
        IsTransportError = false;
        OnPropertyChanged(nameof(DisplayedResponseBody));
    }

    [RelayCommand]
    private async Task CopyResponseBody()
    {
        var text = DisplayedResponseBody;
        if (!string.IsNullOrEmpty(text))
        {
            _dialogService.SetClipboardText(text);
            CopyButtonText = "✓ Copied!";
            await Task.Delay(1500);
            CopyButtonText = "Copy";
        }
    }

    [RelayCommand]
    private void CopyAsCurl() => CopySnippet(CodeSnippetGenerator.ToCurl);

    [RelayCommand]
    private void CopyAsPython() => CopySnippet(CodeSnippetGenerator.ToPythonRequests);

    [RelayCommand]
    private void CopyAsJsFetch() => CopySnippet(CodeSnippetGenerator.ToJsFetch);

    [RelayCommand]
    private void ShowPretty() => IsPrettyResponse = true;

    [RelayCommand]
    private void ShowRaw() => IsPrettyResponse = false;

    public RequestTab ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Method = Method,
        Url = Url,
        Headers = new ObservableCollection<PostmanCloneLibrary.Models.KeyValuePair>(Headers.Select(h => h.ToModel())),
        QueryParams = new ObservableCollection<PostmanCloneLibrary.Models.KeyValuePair>(QueryParams.Select(p => p.ToModel())),
        BodyType = BodyType,
        Body = BodyContent,
        FormData = new ObservableCollection<PostmanCloneLibrary.Models.KeyValuePair>(FormData.Select(f => f.ToModel())),
        Auth = new AuthConfig
        {
            Type = AuthType,
            BearerToken = BearerToken,
            BasicUsername = BasicUsername,
            BasicPassword = BasicPassword,
            ApiKeyName = ApiKeyName,
            ApiKeyValue = ApiKeyValue,
            ApiKeyLocation = ApiKeyLocation
        },
        DisableSslVerification = DisableSslVerification,
        IsDirty = IsDirty
    };

    public void LoadFrom(RequestTab model)
    {
        _isSyncingUrlAndParams = true;
        try
        {
            Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            Name = model.Name;
            Method = string.IsNullOrWhiteSpace(model.Method) ? "GET" : model.Method;
            Url = model.Url ?? string.Empty;
            BodyType = string.IsNullOrWhiteSpace(model.BodyType) ? "none" : model.BodyType;
            BodyContent = model.Body ?? string.Empty;
            DisableSslVerification = model.DisableSslVerification;

            var auth = model.Auth ?? new AuthConfig();
            AuthType = string.IsNullOrWhiteSpace(auth.Type) ? "none" : auth.Type;
            BearerToken = auth.BearerToken;
            BasicUsername = auth.BasicUsername;
            BasicPassword = auth.BasicPassword;
            ApiKeyName = string.IsNullOrWhiteSpace(auth.ApiKeyName) ? "X-API-Key" : auth.ApiKeyName;
            ApiKeyValue = auth.ApiKeyValue;
            ApiKeyLocation = string.IsNullOrWhiteSpace(auth.ApiKeyLocation) ? "header" : auth.ApiKeyLocation;

            Replace(Headers, model.Headers);
            Replace(QueryParams, model.QueryParams);
            Replace(FormData, model.FormData);

            if (Headers.Count == 0) AddEmptyHeader();
            if (QueryParams.Count == 0) AddEmptyParam();

            IsDirty = false;
            NotifyBadges();
        }
        finally
        {
            _isSyncingUrlAndParams = false;
        }
    }

    private void CopySnippet(Func<RequestTab, string> generator)
    {
        var variables = ResolveVariables?.Invoke() ?? new Dictionary<string, string>();
        var composed = RequestComposer.Compose(ToModel(), variables);
        _dialogService.SetClipboardText(generator(composed.Request));
    }

    private void ApplyResponse(ResponseData resp)
    {
        ResponseStatus = resp.StatusCode;
        ResponseStatusText = resp.StatusText;
        ResponseElapsedMs = resp.ElapsedMs;
        ResponseSize = JsonFormatter.FormatSize(resp.SizeBytes);
        ResponseIsSuccess = resp.IsSuccess;
        ResponseContentType = resp.ContentType;
        IsTransportError = resp.IsTransportError;
        TransportErrorMessage = resp.FriendlyError;
        RawResponseBody = resp.Body ?? string.Empty;

        if (resp.IsTransportError)
        {
            PrettyResponseBody = RawResponseBody;
        }
        else if (JsonFormatter.IsBinary(resp.ContentType))
        {
            PrettyResponseBody = RawResponseBody = $"[Binary response — {ResponseSize} of {resp.ContentType}. Not displayed.]";
        }
        else if (JsonFormatter.IsJson(resp.ContentType) || JsonFormatter.LooksLikeJson(RawResponseBody))
        {
            PrettyResponseBody = JsonFormatter.TryFormat(RawResponseBody);
        }
        else if (JsonFormatter.IsXml(resp.ContentType))
        {
            PrettyResponseBody = JsonFormatter.TryFormatXml(RawResponseBody);
        }
        else
        {
            PrettyResponseBody = RawResponseBody;
        }

        ResponseHeaders.Clear();
        foreach (var h in resp.Headers)
            ResponseHeaders.Add(new ResponseHeaderItemViewModel { Key = h.Key, Value = h.Value });

        HasResponse = true;
        ActiveResponseTab = "Body";
        OnPropertyChanged(nameof(DisplayedResponseBody));
        OnPropertyChanged(nameof(ShouldHighlightJson));
    }

    private void ApplyTransportError(string title, string message)
    {
        var resp = new ResponseData
        {
            StatusCode = 0,
            StatusText = title,
            IsTransportError = true,
            FriendlyError = message,
            Body = message,
            ErrorKind = "unknown"
        };
        ApplyResponse(resp);
    }

    private void AddEmptyHeader() => Headers.Add(new KeyValueItemViewModel());

    private void AddEmptyParam() => QueryParams.Add(new KeyValueItemViewModel());

    private void Replace(ObservableCollection<KeyValueItemViewModel> target, IEnumerable<PostmanCloneLibrary.Models.KeyValuePair>? source)
    {
        foreach (var existing in target)
            existing.PropertyChanged -= ItemChanged;
        target.Clear();
        if (source is null) return;
        foreach (var item in source)
            target.Add(KeyValueItemViewModel.FromModel(item));
    }

    private void Track(ObservableCollection<KeyValueItemViewModel> items)
    {
        items.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (KeyValueItemViewModel item in e.NewItems)
                    item.PropertyChanged += ItemChanged;
            }
            if (e.OldItems is not null && e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (KeyValueItemViewModel item in e.OldItems)
                    item.PropertyChanged -= ItemChanged;
            }
            MarkDirty();
            NotifyBadges();
            if (ReferenceEquals(items, QueryParams))
                SyncQueryParamsToUrl();
        };
    }

    private void ItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        MarkDirty();
        NotifyBadges();
        if (sender is KeyValueItemViewModel item && QueryParams.Contains(item))
            SyncQueryParamsToUrl();
    }

    private void MarkDirty()
    {
        IsDirty = true;
        Changed?.Invoke();
    }
}

public class ResponseHeaderItemViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
