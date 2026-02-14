using System;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

/// <summary>
/// Extension methods to register region views and RegionManager with the service collection.
/// </summary>
public static class RegionServiceCollectionExtensions
{
    /// <summary>
    /// Adds region view registry and configures it. Call this, then register <see cref="IRegionManager"/>.
    /// </summary>
    /// <example>
    /// services.AddRegionViews(reg => {
    ///   reg.AddView&lt;PageA&gt;("GridA", ServiceLifetime.Scoped);
    ///   reg.AddView&lt;PageB&gt;("GridB", ServiceLifetime.Scoped);
    /// });
    /// services.AddSingleton&lt;IRegionManager&gt;(sp => new RegionManager(sp, sp.GetRequiredService&lt;IRegionViewRegistry&gt;()));
    /// </example>
    public static IServiceCollection AddRegionViews(this IServiceCollection services, Action<IRegionViewRegistry> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        RegionViewRegistry registry = new RegionViewRegistry(services);
        configure(registry);
        services.AddSingleton<IRegionViewRegistry>(registry);
        return services;
    }

    /// <summary>
    /// Registers region views and <see cref="IRegionManager"/>.
    /// Convenience method that calls AddRegionViews and adds IRegionManager.
    /// Register <see cref="IRegionHostContentAdapter"/> before this to customize how view is displayed in the host.
    /// </summary>
    public static IServiceCollection AddLiteObservableRegions(this IServiceCollection services, Action<IRegionViewRegistry> configure)
    {
        services.AddRegionViews(configure);
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            sp.GetService<IRegionHostContentAdapter>()));
        return services;
    }
}
