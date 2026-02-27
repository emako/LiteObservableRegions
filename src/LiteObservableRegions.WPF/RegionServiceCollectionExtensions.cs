using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1510 // Use 'ArgumentNullException.ThrowIfNull' instead of explicitly throwing a new exception instance

namespace LiteObservableRegions;

/// <summary>
/// Extension methods to register region views and RegionManager with the service collection.
/// </summary>
public static class RegionServiceCollectionExtensions
{
    /// <summary>
    /// Adds region view registry and configures it. Call this, then register <see cref="IRegionManager"/> manually if you need custom constructor parameters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to register view types (e.g. reg.AddView&lt;PageA&gt;("ViewA", ServiceLifetime.Scoped)).</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    /// <example>
    /// services.AddRegionViews(reg => {
    ///   reg.AddView&lt;PageA&gt;("GridA", ServiceLifetime.Scoped);
    ///   reg.AddView&lt;PageB&gt;("GridB", ServiceLifetime.Scoped);
    /// });
    /// services.AddSingleton&lt;IRegionManager&gt;(sp => new RegionManager(sp, sp.GetRequiredService&lt;IRegionViewRegistry&gt;()));
    /// </example>
    public static IServiceCollection AddRegionViews(this IServiceCollection services, Action<IRegionViewRegistry> configure)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        RegionViewRegistry registry = new(services);
        configure(registry);
        services.AddSingleton<IRegionViewRegistry>(registry);
        return services;
    }

    /// <summary>
    /// Registers region views and <see cref="IRegionManager"/>.
    /// Convenience method that calls <see cref="AddRegionViews"/> and adds <see cref="IRegionManager"/> with optional <see cref="IRegionHostContentAdapter"/> and region-changing callback.
    /// Register <see cref="IRegionHostContentAdapter"/> before this to customize how the view is displayed in the host.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to register view types.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddObservableRegions(this IServiceCollection services, Action<IRegionViewRegistry> configure)
    {
        services.AddRegionViews(configure);
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            sp.GetService<IRegionHostContentAdapter>(),
            onRegionChanging: RegionChangedHubCallback));
        return services;
    }

    private static void RegionChangedHubCallback(RegionChangedEventArgs e)
    {
        WeakReferenceRegionHub.RaiseRegionChanged(e);
    }
}

#pragma warning restore CA1510 // Use 'ArgumentNullException.ThrowIfNull' instead of explicitly throwing a new exception instance
#pragma warning restore IDE0079 // Remove unnecessary suppression
