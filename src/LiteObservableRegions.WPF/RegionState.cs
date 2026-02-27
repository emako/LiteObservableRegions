using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LiteObservableRegions;

/// <summary>
/// State for a single region (host, back/forward stacks, named views, optional scoped service scope).
/// Exposed via <see cref="IRegionManager.Regions"/>.
/// </summary>
public sealed class RegionState(DependencyObject host)
{
    /// <summary>
    /// The host element that displays the region content (the element with <see cref="ObservableRegion.RegionName"/>).
    /// </summary>
    public DependencyObject Host { get; } = host ?? throw new ArgumentNullException(nameof(host));

    /// <summary>
    /// Back stack (previous entries when navigating forward). Top is the most recent back target.
    /// </summary>
    public Stack<NavigationEntry> BackStack { get; } = new Stack<NavigationEntry>();

    /// <summary>
    /// Forward stack (entries when going back). Top is the next forward target.
    /// </summary>
    public Stack<NavigationEntry> ForwardStack { get; } = new Stack<NavigationEntry>();

    /// <summary>
    /// The current navigation entry; null if no navigation has occurred yet.
    /// </summary>
    public NavigationEntry CurrentEntry { get; set; }

    /// <summary>
    /// Optional service scope for Scoped view resolution. Created on first use; disposed when the region is replaced or host unloads.
    /// </summary>
    public IServiceScope Scope { get; set; }

    /// <summary>
    /// View name -> view instance (weak). Filled by <see cref="IRegionManager.RegisterNamedView"/> (e.g. from <see cref="ObservableRegion.ViewName"/>).
    /// </summary>
    public Dictionary<string, WeakReference> NamedViews { get; } = new Dictionary<string, WeakReference>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Disposes the scoped service scope and sets <see cref="Scope"/> to null.
    /// Called when the region is replaced or the host is unloaded.
    /// </summary>
    public void DisposeScope()
    {
        Scope?.Dispose();
        Scope = null;
    }
}
