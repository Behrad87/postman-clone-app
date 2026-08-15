namespace PostmanCloneLibrary.Models;

public class HistoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public long ElapsedMs { get; set; }
    public RequestTab? RequestSnapshot { get; set; }
}
