using CommunityToolkit.Mvvm.ComponentModel;

using PostmanCloneLibrary.Models;

using System.Collections.ObjectModel;

using AppEnvironment = PostmanCloneLibrary.Models.Environment;

namespace PostmanCloneWPFUI.ViewModels;

public partial class EnvironmentViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string _name = "Development";
    [ObservableProperty] private bool _isGlobals;
    [ObservableProperty] private bool _disableSslVerification;

    public ObservableCollection<EnvironmentVariableViewModel> Variables { get; } = [];

    public AppEnvironment ToModel() => new()
    {
        Id = Id,
        Name = Name,
        IsGlobals = IsGlobals,
        DisableSslVerification = DisableSslVerification,
        Variables = new ObservableCollection<EnvironmentVariable>(Variables.Select(v => v.ToModel()))
    };

    public static EnvironmentViewModel FromModel(AppEnvironment model)
    {
        var vm = new EnvironmentViewModel
        {
            Id = model.Id,
            Name = model.Name,
            IsGlobals = model.IsGlobals,
            DisableSslVerification = model.DisableSslVerification
        };
        foreach (var v in model.Variables)
            vm.Variables.Add(EnvironmentVariableViewModel.FromModel(v));
        return vm;
    }
}

public partial class EnvironmentVariableViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = string.Empty;
    [ObservableProperty] private bool _isSecret;

    public EnvironmentVariable ToModel() => new()
    {
        Id = Id,
        Key = Key,
        Value = Value,
        IsSecret = IsSecret
    };

    public static EnvironmentVariableViewModel FromModel(EnvironmentVariable model) => new()
    {
        Id = model.Id,
        Key = model.Key,
        Value = model.Value,
        IsSecret = model.IsSecret
    };
}
