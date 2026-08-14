using System.Collections.ObjectModel;

namespace PostmanCloneLibrary.Models;

public class Environment
{
    public static readonly Guid GlobalsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Development";
    public bool IsGlobals { get; set; }
    public bool DisableSslVerification { get; set; }
    public ObservableCollection<EnvironmentVariable> Variables { get; set; } = [];
}
