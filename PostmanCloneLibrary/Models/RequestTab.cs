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
    /// <summary>none | json | text | xml | form | multipart</summary>
    public string BodyType { get; set; } = "none";
    public ObservableCollection<KeyValuePair> FormData { get; set; } = [];
    public AuthConfig Auth { get; set; } = new();
    public bool DisableSslVerification { get; set; }
    public ResponseData? Response { get; set; }
    public bool IsDirty { get; set; }

    public RequestTab Clone(bool includeResponse = false)
    {
        return new RequestTab
        {
            Id = Id,
            Name = Name,
            Method = Method,
            Url = Url,
            Headers = ClonePairs(Headers),
            QueryParams = ClonePairs(QueryParams),
            Body = Body,
            BodyType = BodyType,
            FormData = ClonePairs(FormData),
            Auth = Auth.Clone(),
            DisableSslVerification = DisableSslVerification,
            Response = includeResponse ? Response : null,
            IsDirty = IsDirty
        };
    }

    private static ObservableCollection<KeyValuePair> ClonePairs(IEnumerable<KeyValuePair> source)
        => new(source.Select(p => p.Clone()));
}
