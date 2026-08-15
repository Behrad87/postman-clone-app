namespace PostmanCloneLibrary.Models;

public class KeyValuePair
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsEnabled { get; set; } = true;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>text | file (used by multipart form-data)</summary>
    public string Type { get; set; } = "text";
    public string FilePath { get; set; } = string.Empty;

    public KeyValuePair Clone() => new()
    {
        Id = Id,
        IsEnabled = IsEnabled,
        Key = Key,
        Value = Value,
        Description = Description,
        Type = Type,
        FilePath = FilePath
    };
}
