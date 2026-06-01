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
}
