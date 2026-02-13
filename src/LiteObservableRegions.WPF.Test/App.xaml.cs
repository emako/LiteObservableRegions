using LiteObservableRegions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using LiteObservableRegions.WPF.Test.Views;

namespace LiteObservableRegions.WPF.Test;

/// <summary>
/// WPF test application that sets up DI and LiteObservableRegions.
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(services);

        services.AddLiteObservableRegions(reg =>
        {
            reg.AddView<PageA>("GridA", ServiceLifetime.Scoped);
            reg.AddView<PageB>("GridB", ServiceLifetime.Scoped);
        });

        ServiceProvider = services.BuildServiceProvider();
        RegionServiceProvider.Current = ServiceProvider;
    }
}
