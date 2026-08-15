using System.Collections.ObjectModel;

namespace PostmanCloneLibrary.Models;

public class Collection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Collection";
    public ObservableCollection<SavedRequest> Requests { get; set; } = [];
    public bool IsExpanded { get; set; } = true;
}
