using Microsoft.Extensions.DependencyInjection;
using Regions.Abstractions;
using Regions.Test.Views;
using System.Windows;

namespace Regions.Test;

/// <summary>
/// WPF test application that sets up DI and Regions infrastructure.
/// WPF 测试应用：配置依赖注入与 Regions 基础设施。
/// </summary>
public partial class App : Application, IServiceProvider
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// IServiceProvider implementation proxying to the configured ServiceProvider.
    /// IServiceProvider 实现：代理到已配置的 ServiceProvider。
    /// </summary>
    public object? GetService(Type serviceType)
    {
        return ServiceProvider!.GetService(serviceType);
    }

    /// <summary>
    /// Gets a required service using the underlying ServiceProvider.
    /// 使用内部的 ServiceProvider 获取必需服务。
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        return ServiceProvider!.GetRequiredService<T>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(services);
        services.AddSingleton<IRegionManager, RegionManager>();
        services.AddSingleton<INavigationRegistry, NavigationRegistry>();

        // Test with Singleton lifetime
        //services.AddSingleton<PageA>();
        //services.AddSingleton<PageB>();

        // Test with Region Scoped lifetime
        services.AddScoped<PageA>();
        services.AddScoped<PageB>();

        // Test with Transient lifetime
        //services.AddTransient<PageA>();
        //services.AddTransient<PageB>();

        ServiceProvider = services.BuildServiceProvider()
            .UseRegionServiceProvider()
            .RegisterNavigationRegistry(registry =>
            {
                // Test for ContentControl
                registry.Register<PageA>("/PageA");
                registry.Register<PageB>("/PageB");

                // Test for Grid
                registry.Register<PageA>("/GridA");
                registry.Register<PageB>("/GridB");
            });
    }
}
