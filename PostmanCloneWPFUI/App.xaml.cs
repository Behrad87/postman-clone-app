using System.Windows;

using PostmanCloneLibrary;
using PostmanCloneLibrary.Persistence;
using PostmanCloneWPFUI.ViewModels;

namespace PostmanCloneWPFUI;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        var store = new JsonAppStore();
        var api = new ApiAccess();
        var vm = new MainViewModel(api, store);
        await vm.InitializeAsync();

        var window = new MainWindow(vm);
        MainWindow = window;
        window.Show();
    }
}
