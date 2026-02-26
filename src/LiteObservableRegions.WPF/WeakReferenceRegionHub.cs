using LiteObservableRegions.Abstractions;
using System;

namespace LiteObservableRegions;

public class WeakReferenceRegionHub
{
    internal static readonly RegionServiceProvider _regionServiceProvider = new();

    public static IServiceProvider ServiceProvider
    {
        get => _regionServiceProvider.Current;
        set => _regionServiceProvider.Current = value;
    }

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
    /// </summary>
    public static void Clear()
        => RegionManager?.Clear();
}
