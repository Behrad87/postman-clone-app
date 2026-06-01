using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Text;

namespace PostmanCloneWPFUI.ViewModels;


public partial class KeyValueItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = string.Empty;
    [ObservableProperty] private string _description = string.Empty;

    public Guid Id { get; } = Guid.NewGuid();

    public PostmanCloneLibrary.Models.KeyValuePair ToModel() => new()
    {
        Id = Id,
        IsEnabled = IsEnabled,
        Key = Key,
        Value = Value,
        Description = Description
    };

    public static KeyValueItemViewModel FromModel(PostmanCloneLibrary.Models.KeyValuePair m) => new()
    {
        IsEnabled = m.IsEnabled,
        Key = m.Key,
        Value = m.Value,
        Description = m.Description
    };
}
