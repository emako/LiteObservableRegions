using System;

namespace LiteObservableRegions;

public sealed class RegionEventNotifier
{
    /// <summary>
    /// Raised before a region's content changes (Navigate, Redirect, GoBack, GoForward).
    /// Subscribe to get detailed context (region name, from/to URI and target names, mode) and optionally cancel by setting <see cref="RegionChangedEventArgs.Cancel"/>.
    /// </summary>
    public event EventHandler<RegionChangedEventArgs> ObservableRegionChanged;

    /// <summary>
    /// Called by RegionManager when a region change is about to occur. Invokes <see cref="ObservableRegionChanged"/>.
    /// </summary>
    internal void RaiseRegionChanged(RegionChangedEventArgs e)
        => ObservableRegionChanged?.Invoke(this, e);
}
