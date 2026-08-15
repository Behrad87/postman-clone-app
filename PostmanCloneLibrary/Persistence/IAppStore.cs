using PostmanCloneLibrary.Models;

namespace PostmanCloneLibrary.Persistence;

public interface IAppStore
{
    Task<AppState> LoadAsync();
    void ScheduleSave(AppState state);
    Task SaveNowAsync(AppState state);
}
