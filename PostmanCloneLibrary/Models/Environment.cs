using System.Collections.ObjectModel;

namespace PostmanCloneLibrary.Models;

public class Environment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Development";
    public ObservableCollection<EnvironmentVariable> Variables { get; set; } = [];
}