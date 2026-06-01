namespace PostmanCloneLibrary.Models;

public class SavedRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public RequestTab RequestData { get; set; } = new();
}
