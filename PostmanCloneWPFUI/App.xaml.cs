using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PostmanCloneLibrary;
using PostmanCloneLibrary.Persistence;
using PostmanCloneWPFUI.Services;
using PostmanCloneWPFUI.ViewModels;

namespace PostmanCloneWPFUI;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public static IServiceProvider Services =>
        ((App)Current)._serviceProvider ?? throw new InvalidOperationException("Services not initialized.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        var window = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = window;
        window.Show();

        await vm.InitializeAsync();
        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IApiAccess, ApiAccess>();
        services.AddSingleton<IAppStore, JsonAppStore>();
        services.AddSingleton<IDialogService, WpfDialogService>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<RequestTabViewModel>();
        services.AddSingleton<MainWindow>();
    }
}
