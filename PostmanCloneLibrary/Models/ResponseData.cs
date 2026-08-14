namespace PostmanCloneLibrary.Models;

public class ResponseData
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public long ElapsedMs { get; set; }
    public long SizeBytes { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public bool IsTransportError { get; set; }
    /// <summary>timeout | cancelled | dns | connection | ssl | unknown</summary>
    public string ErrorKind { get; set; } = string.Empty;
    public string FriendlyError { get; set; } = string.Empty;
}
