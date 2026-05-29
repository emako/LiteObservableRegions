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
    /// Raised when <see cref="Abstractions.IRegionNavigation.CanGo"/> is called to query whether navigation to a URI is permitted.
    /// Subscribe and set <see cref="RegionCanGoEventArgs.CanGo"/> to <c>false</c> to indicate the navigation should not proceed.
    /// This event does <b>not</b> affect Navigate, Redirect, GoBack, or GoForward — those always execute unless you check <see cref="Abstractions.IRegionNavigation.CanGo"/> yourself first.
    /// </summary>
    public event EventHandler<RegionCanGoEventArgs> ObservableRegionCanGo;

    /// <summary>
    /// Called by RegionManager when a region change is about to occur. Invokes <see cref="ObservableRegionChanged"/>.
    /// </summary>
    internal void RaiseRegionChanged(RegionChangedEventArgs e)
        => ObservableRegionChanged?.Invoke(this, e);

    /// <summary>
    /// Called by RegionManager when <see cref="Abstractions.IRegionNavigation.CanGo"/> is queried.
    /// Invokes <see cref="ObservableRegionCanGo"/> and returns <c>false</c> if any subscriber set <see cref="RegionCanGoEventArgs.CanGo"/> to <c>false</c>.
    /// </summary>
    public bool RaiseCanGo(Uri uri)
    {
        RegionCanGoEventArgs args = new(uri);
        ObservableRegionCanGo?.Invoke(this, args);
        return args.CanGo;
    }
}
