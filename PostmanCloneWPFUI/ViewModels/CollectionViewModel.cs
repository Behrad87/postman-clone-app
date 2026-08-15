using CommunityToolkit.Mvvm.ComponentModel;

using PostmanCloneLibrary.Models;

using System.Collections.ObjectModel;

namespace PostmanCloneWPFUI.ViewModels;

public partial class CollectionViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string _name = "Collection";
    [ObservableProperty] private bool _isExpanded = true;

    public ObservableCollection<SavedRequestViewModel> Requests { get; } = [];

    public Collection ToModel() => new()
    {
        Id = Id,
        Name = Name,
        IsExpanded = IsExpanded,
        Requests = new ObservableCollection<SavedRequest>(Requests.Select(r => r.ToModel()))
    };

    public static CollectionViewModel FromModel(Collection model)
    {
        var vm = new CollectionViewModel
        {
            Id = model.Id,
            Name = model.Name,
            IsExpanded = model.IsExpanded
        };
        foreach (var r in model.Requests)
            vm.Requests.Add(SavedRequestViewModel.FromModel(r));
        return vm;
    }
}
