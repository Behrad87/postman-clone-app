using Microsoft.Extensions.DependencyInjection;

using PostmanCloneLibrary;

using PostmanCloneWPFUI.ViewModels;
using PostmanCloneWPFUI.Views;

namespace PostmanCloneWPFUI;

public static class ServiceLocator
{
    private static IServiceProvider? _provider;

    public static IServiceProvider Provider =>
        _provider ?? throw new InvalidOperationException("ServiceLocator not initialized.");

    public static void Build()
    {
        var services = new ServiceCollection();

        // Infrastructure
        services.AddSingleton<IApiAccess, ApiAccess>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<RequestTabViewModel>();

        // Factory: lets MainViewModel create new tabs without knowing DI
        services.AddSingleton<Func<RequestTabViewModel>>(sp =>
            () => sp.GetRequiredService<RequestTabViewModel>());

        // Views — registered LAST so their dependencies are already known
        services.AddSingleton<MainWindow>();

        _provider = services.BuildServiceProvider();
    }

    public static T Get<T>() where T : notnull =>
        Provider.GetRequiredService<T>();
}