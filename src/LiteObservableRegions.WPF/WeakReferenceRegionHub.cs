using LiteObservableRegions.Abstractions;
using System;

namespace LiteObservableRegions;

/// <summary>
/// Static hub for the application's region service provider and region change notifications.
/// Set <see cref="ServiceProvider"/> after building the DI container so that XAML-attached region/view registration can resolve <see cref="IRegionManager"/>.
/// </summary>
public class WeakReferenceRegionHub
{
    internal static readonly RegionServiceProvider _regionServiceProvider = new();

    /// <summary>
    /// The application's service provider. Set by the host (e.g. in App.OnStartup) after building the DI container.
    /// </summary>
    public static IServiceProvider ServiceProvider
    {
        get => _regionServiceProvider.Current;
        set => _regionServiceProvider.Current = value;
    }

    /// <summary>
    /// Gets the registered <see cref="IRegionManager"/> from the current service provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">ServiceProvider is not set or IRegionManager is not registered.</exception>
    public static IRegionManager RegionManager
        => _regionServiceProvider.GetRequiredService<IRegionManager>();

    /// <summary>
    /// Raised before a region's content changes (Navigate, Redirect, GoBack, GoForward).
    /// Subscribe to get detailed context (region name, from/to URI and target names, mode) and optionally cancel by setting <see cref="RegionChangedEventArgs.Cancel"/>.
    /// </summary>
    public static event EventHandler<RegionChangedEventArgs> ObservableRegionChanged;

    /// <summary>
    /// Called by RegionManager when a region change is about to occur. Invokes <see cref="ObservableRegionChanged"/>.
    /// </summary>
    internal static void RaiseRegionChanged(RegionChangedEventArgs e)
        => ObservableRegionChanged?.Invoke(null, e);

    /// <summary>
    /// Clears all named views (per region) and the singleton view cache. No-op if the service provider is not set.
    /// Does not unregister regions or navigation stacks.
    /// </summary>
    public static void Clear()
        => RegionManager?.Clear();
}
