using CommunityToolkit.Mvvm.ComponentModel;

namespace PostmanCloneWPFUI.ViewModels;

public partial class KeyValueItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _itemType = "text";
    [ObservableProperty] private string _filePath = string.Empty;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName => string.IsNullOrWhiteSpace(FilePath)
        ? "Choose file…"
        : System.IO.Path.GetFileName(FilePath);

    partial void OnFilePathChanged(string value) => OnPropertyChanged(nameof(FileName));

    public PostmanCloneLibrary.Models.KeyValuePair ToModel() => new()
    {
        Id = Id,
        IsEnabled = IsEnabled,
        Key = Key,
        Value = Value,
        Description = Description,
        Type = ItemType,
        FilePath = FilePath
    };

    public static KeyValueItemViewModel FromModel(PostmanCloneLibrary.Models.KeyValuePair m) => new()
    {
        Id = m.Id,
        IsEnabled = m.IsEnabled,
        Key = m.Key,
        Value = m.Value,
        Description = m.Description,
        ItemType = string.IsNullOrWhiteSpace(m.Type) ? "text" : m.Type,
        FilePath = m.FilePath
    };
}
